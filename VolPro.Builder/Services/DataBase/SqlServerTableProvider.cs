using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using VolPro.Builder.IServices;
using VolPro.Core.EFDbContext;
namespace VolPro.Builder.Services;

public class SqlServerTableProvider : ITableDatabaseProvider
{
    private readonly BaseDbContext _context;

    public SqlServerTableProvider(BaseDbContext context)
    {
        _context = context;
    }

    private static string NormalizeSqlServerDefault(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        var s = raw.Trim();
        while (s.StartsWith("(") && s.EndsWith(")")) s = s[1..^1].Trim();
        return string.IsNullOrWhiteSpace(s) ? null : s;
    }

    private static string FormatDefaultForSqlServer(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        var v = value.Trim();
        var vLower = v.ToLower();
        if (vLower is "getdate()" or "newid()" or "sysdatetime()")
            return $" DEFAULT {v}";
        if (long.TryParse(v, out _) || decimal.TryParse(v, out _))
            return $" DEFAULT {v}";
        if (vLower == "true" || vLower == "1") return " DEFAULT 1";
        if (vLower == "false" || vLower == "0") return " DEFAULT 0";
        return $" DEFAULT N'{v.Replace("'", "''")}'";
    }

    private static string BuildColumnDefinition(TableColumnDto column)
    {
        var definition = $"[{column.ColumnName}] ";
        var dtLower = (column.DataType ?? "").Trim().ToLower();
        if (dtLower == "nvarchar(max)") { dtLower = "nvarchar"; }
        if (dtLower == "varchar(max)") { dtLower = "varchar"; }
        if (dtLower is "nvarchar" or "varchar")
        {
            var length = (column.DataType ?? "").Contains("max", StringComparison.OrdinalIgnoreCase) ? -1 : (column.Length ?? 255);
            definition += $"{dtLower}({(length == -1 ? "MAX" : length.ToString())})";
        }
        else if (dtLower is "char" or "nchar")
        {
            var length = column.Length ?? 1;
            definition += $"{column.DataType}({length})";
        }
        else if (dtLower is "decimal" or "numeric")
        {
            var precision = column.Length ?? 18;
            var scale = column.Scale ?? 0;
            definition += $"{column.DataType}({precision}, {scale})";
        }
        else
        {
            definition += column.DataType ?? "nvarchar";
        }
        if (column.IsIdentity) definition += " IDENTITY(1,1)";
        if (!column.IsNullable) definition += " NOT NULL";
        definition += FormatDefaultForSqlServer(column.DefaultValue);
        return definition;
    }

    /// <summary>
    /// SQL Server 不支持 ALTER COLUMN 修改 IDENTITY，通过临时表重建实现
    /// </summary>
    private async Task RebuildTableForIdentityChangeAsync(UpdateTableRequest request, Dictionary<string, TableColumnDto> existingByName)
    {
        var tableName = request.TableName;
        var tmpName = $"Tmp_{tableName}_{Guid.NewGuid().ToString("N")[..8]}";
        var colDefs = request.Columns.OrderBy(c => c.Order).Select(BuildColumnDefinition).ToList();
        var createTmpSql = $"CREATE TABLE [{tmpName}] (\n    " + string.Join(",\n    ", colDefs) + "\n)";
        await _context.SqlSugarClient.Ado.ExecuteCommandAsync(createTmpSql);

        var colsToCopy = request.Columns.Where(c => existingByName.ContainsKey(c.ColumnName)).OrderBy(c => c.Order).Select(c => c.ColumnName).ToList();
        var hasIdentity = request.Columns.Any(c => c.IsIdentity);
        var colList = string.Join(", ", colsToCopy.Select(c => $"[{c}]"));

        // IDENTITY_INSERT 是会话级，必须与 INSERT 在同一连接/同一批中执行
        if (colsToCopy.Count > 0)
        {
            var insertBatch = hasIdentity
                ? $"SET IDENTITY_INSERT [{tmpName}] ON; INSERT INTO [{tmpName}] ({colList}) SELECT {colList} FROM [{tableName}]; SET IDENTITY_INSERT [{tmpName}] OFF"
                : $"INSERT INTO [{tmpName}] ({colList}) SELECT {colList} FROM [{tableName}]";
            await _context.SqlSugarClient.Ado.ExecuteCommandAsync(insertBatch);
        }

        var dropPkSql = $@"
            DECLARE @pk NVARCHAR(200);
            SELECT @pk = name FROM sys.key_constraints WHERE type = 'PK' AND parent_object_id = OBJECT_ID('{tableName.Replace("'", "''")}');
            IF @pk IS NOT NULL EXEC('ALTER TABLE [{tableName}] DROP CONSTRAINT [' + @pk + ']')";
        await _context.SqlSugarClient.Ado.ExecuteCommandAsync(dropPkSql);

        await _context.SqlSugarClient.Ado.ExecuteCommandAsync($"DROP TABLE [{tableName}]");
        await _context.SqlSugarClient.Ado.ExecuteCommandAsync($"EXEC sp_rename 'dbo.[{tmpName}]', '{tableName.Replace("'", "''")}', 'OBJECT'");

        if (request.Columns.Any(c => c.IsPrimaryKey))
        {
            var pkCols = request.Columns.Where(c => c.IsPrimaryKey).OrderBy(c => c.Order).Select(c => $"[{c.ColumnName}]");
            var addPkSql = $"ALTER TABLE [{tableName}] ADD CONSTRAINT [PK_{tableName}] PRIMARY KEY ({string.Join(", ", pkCols)})";
            await _context.SqlSugarClient.Ado.ExecuteCommandAsync(addPkSql);
        }

        foreach (var column in request.Columns.Where(c => !string.IsNullOrWhiteSpace(c.Comment)))
        {
            try
            {
                var commentSql = $@"
                    EXEC sp_addextendedproperty @name = N'MS_Description', @value = N'{column.Comment.Replace("'", "''")}',
                    @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'{tableName.Replace("'", "''")}', @level2type = N'COLUMN', @level2name = N'{column.ColumnName.Replace("'", "''")}'";
                await _context.SqlSugarClient.Ado.ExecuteCommandAsync(commentSql);
            }
            catch { /* 注释已存在时忽略 */ }
        }
    }

    public async Task<bool> TableExistsAsync(string tableName)
    {
        const string sql = "SELECT COUNT(*) as Value FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @tableName";
        var res = await _context.SqlSugarClient.Ado.GetScalarAsync(sql, new { tableName });
        return Convert.ToInt32(res) > 0;
    }

    public async Task CreateTableAsync(CreateTableRequest request)
    {
        var columnDefinitions = new List<string>();

        foreach (var column in request.Columns.OrderBy(c => c.Order))
        {
            var definition = $"[{column.ColumnName}] ";

            // Data type and length
            var dtLower = column.DataType.ToLower();
            if (dtLower == "nvarchar" || dtLower == "varchar")
            {
                var length = column.Length ?? 255;
                definition += $"{column.DataType}({(length == -1 ? "MAX" : length.ToString())})";
            }
            else if (dtLower == "char" || dtLower == "nchar")
            {
                var length = column.Length ?? 1;
                definition += $"{column.DataType}({length})";
            }
            else if (dtLower == "decimal" || dtLower == "numeric")
            {
                var precision = column.Length ?? 18;
                var scale = column.Scale ?? 0;
                definition += $"{column.DataType}({precision}, {scale})";
            }
            else
            {
                definition += column.DataType;
            }

            // Identity (Auto increment)
            if (column.IsIdentity)
            {
                definition += " IDENTITY(1,1)";
            }

            // Nullable
            if (!column.IsNullable)
            {
                definition += " NOT NULL";
            }

            // Default value
            definition += FormatDefaultForSqlServer(column.DefaultValue);

            columnDefinitions.Add(definition);
        }

        // Build CREATE TABLE statement
        var createTableSql = $"CREATE TABLE [{request.TableName}] (\n    " +
                            string.Join(",\n    ", columnDefinitions) +
                            "\n)";

        await _context.SqlSugarClient.Ado.ExecuteCommandAsync(createTableSql);

        // Add primary key constraint
        var pkColumns = request.Columns
            .Where(c => c.IsPrimaryKey)
            .OrderBy(c => c.Order)
            .Select(c => $"[{c.ColumnName}]");

        var pkConstraintName = $"PK_{request.TableName}";
        var pkSql = $"ALTER TABLE [{request.TableName}] ADD CONSTRAINT [{pkConstraintName}] PRIMARY KEY ({string.Join(", ", pkColumns)})";
        await _context.SqlSugarClient.Ado.ExecuteCommandAsync(pkSql);

        // Add column comments (MS_Description)
        foreach (var column in request.Columns.Where(c => !string.IsNullOrWhiteSpace(c.Comment)))
        {
            var commentSql = $@"
                EXEC sp_addextendedproperty 
                    @name = N'MS_Description', 
                    @value = N'{column.Comment.Replace("'", "''")}', 
                    @level0type = N'SCHEMA', @level0name = N'dbo', 
                    @level1type = N'TABLE', @level1name = N'{request.TableName}', 
                    @level2type = N'COLUMN', @level2name = N'{column.ColumnName}'";
            await _context.SqlSugarClient.Ado.ExecuteCommandAsync(commentSql);
        }
    }

    public async Task<List<string>> GetAllTablesAsync()
    {
        const string sql = "SELECT TABLE_NAME as Value FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' ORDER BY TABLE_NAME";
        var rows = await _context.SqlSugarClient.Ado.SqlQueryAsync<string>(sql);
        return rows ?? new List<string>();
    }

    public async Task<TableInfoDto> GetTableInfoAsync(string tableName)
    {
        const string columnsSql = @"
            DECLARE @objId INT = OBJECT_ID(QUOTENAME('dbo') + '.' + QUOTENAME(@tableName));
            SELECT
                c.COLUMN_NAME,
                c.DATA_TYPE,
                c.CHARACTER_MAXIMUM_LENGTH,
                c.IS_NULLABLE,
                c.ORDINAL_POSITION,
                c.NUMERIC_SCALE,
                c.NUMERIC_PRECISION,
                COLUMNPROPERTY(@objId, c.COLUMN_NAME, 'IsIdentity') AS IS_IDENTITY,
                CASE WHEN pk.COLUMN_NAME IS NOT NULL THEN 1 ELSE 0 END AS IS_PRIMARY_KEY,
                CAST(ep.value AS NVARCHAR(4000)) AS COMMENT,
                c.COLUMN_DEFAULT
            FROM INFORMATION_SCHEMA.COLUMNS c
            LEFT JOIN (
                SELECT ku.TABLE_NAME, ku.COLUMN_NAME
                FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
                INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE ku
                    ON tc.CONSTRAINT_TYPE = 'PRIMARY KEY' AND tc.CONSTRAINT_NAME = ku.CONSTRAINT_NAME
            ) pk ON c.TABLE_NAME = pk.TABLE_NAME AND c.COLUMN_NAME = pk.COLUMN_NAME
            LEFT JOIN sys.columns sc ON sc.object_id = @objId AND sc.name = c.COLUMN_NAME
            LEFT JOIN sys.extended_properties ep ON ep.major_id = sc.object_id AND ep.minor_id = sc.column_id AND ep.name = 'MS_Description'
            WHERE c.TABLE_SCHEMA = 'dbo' AND c.TABLE_NAME = @tableName
            ORDER BY c.ORDINAL_POSITION;";

        var dt = await _context.SqlSugarClient.Ado.GetDataTableAsync(columnsSql, new { tableName });
        var columnsWithComments = new List<TableColumnDto>();
        foreach (DataRow row in dt.Rows)
        {
            var length = row[2] == DBNull.Value || row[2] == null ? (int?)null : Convert.ToInt32(row[2]);
            var numericScale = row[5] == DBNull.Value || row[5] == null ? (int?)null : Convert.ToInt32(row[5]);
            var numericPrecision = row[6] == DBNull.Value || row[6] == null ? (int?)null : Convert.ToInt32(row[6]);
            var dataType = row[1]?.ToString() ?? "";
            var isDecimal = dataType.Equals("decimal", StringComparison.OrdinalIgnoreCase) || dataType.Equals("numeric", StringComparison.OrdinalIgnoreCase);
            var colDefault = row[10] == DBNull.Value || row[10] == null ? null : NormalizeSqlServerDefault(row[10]?.ToString());

            columnsWithComments.Add(new TableColumnDto
            {
                ColumnName = row[0]?.ToString() ?? "",
                DataType = dataType,
                Length = isDecimal ? numericPrecision : (length == -1 ? -1 : length),
                Scale = isDecimal ? numericScale : null,
                IsNullable = row[3]?.ToString() == "YES",
                Order = Convert.ToInt32(row[4]),
                IsIdentity = row[7] != DBNull.Value && row[7] != null && Convert.ToInt32(row[7]) == 1,
                IsPrimaryKey = row[8] != DBNull.Value && row[8] != null && Convert.ToInt32(row[8]) == 1,
                Comment = row[9]?.ToString(),
                DefaultValue = colDefault ?? string.Empty
            });
        }

        return new TableInfoDto { TableName = tableName, Columns = columnsWithComments };
    }

    public async Task UpdateTableAsync(UpdateTableRequest request)
    {
        var existingTable = await GetTableInfoAsync(request.TableName);
        if (existingTable == null)
        {
            throw new InvalidOperationException($"Could not retrieve table information for '{request.TableName}'.");
        }

        var existingByName = existingTable.Columns.ToDictionary(c => c.ColumnName, c => c, StringComparer.Ordinal);
        var newColumnNames = request.Columns.Select(c => c.ColumnName).ToHashSet(StringComparer.Ordinal);

        // 1) 识别并执行重命名字段（区分大小写）
        var renames = request.Columns
            .Where(c => c.IsDbField && !string.IsNullOrWhiteSpace(c.OriginalColumnName) &&
                c.OriginalColumnName != c.ColumnName &&
                existingByName.Keys.Any(k => string.Equals(k, c.OriginalColumnName, StringComparison.OrdinalIgnoreCase)))
            .ToList();
        foreach (var r in renames)
        {
            var actualKey = existingByName.Keys.FirstOrDefault(k => string.Equals(k, r.OriginalColumnName, StringComparison.OrdinalIgnoreCase));
            if (actualKey == null) continue;
            var renameSql = $"EXEC sp_rename 'dbo.[{request.TableName}].[{actualKey}]', '{r.ColumnName.Replace("'", "''")}', 'COLUMN';";
            await _context.SqlSugarClient.Ado.ExecuteCommandAsync(renameSql);
            var oldCol = existingByName[actualKey];
            existingByName.Remove(actualKey);
            existingByName[r.ColumnName] = new TableColumnDto { ColumnName = r.ColumnName, DataType = oldCol.DataType, Length = oldCol.Length, Scale = oldCol.Scale, IsNullable = oldCol.IsNullable, IsPrimaryKey = oldCol.IsPrimaryKey, IsIdentity = oldCol.IsIdentity, Comment = oldCol.Comment, DefaultValue = oldCol.DefaultValue ?? "", Order = oldCol.Order };
        }

        var existingColumnNames = existingByName.Keys.ToHashSet(StringComparer.Ordinal);
        var columnsToDrop = existingColumnNames.Except(newColumnNames);
        foreach (var columnName in columnsToDrop)
        {
            var dropColumnSql = $"ALTER TABLE [{request.TableName}] DROP COLUMN [{columnName}]";
            await _context.SqlSugarClient.Ado.ExecuteCommandAsync(dropColumnSql);
        }

        // 2) 新增列 
        var columnsToAdd = request.Columns.Where(c => !existingColumnNames.Contains(c.ColumnName, StringComparer.Ordinal));
        foreach (var column in columnsToAdd.OrderBy(c => c.Order))
        {
            var definition = $"[{column.ColumnName}] ";
            var dtLowerAdd = column.DataType.ToLower();

            if (dtLowerAdd == "nvarchar" || dtLowerAdd == "varchar")
            {
                var length = column.Length ?? 255;
                definition += $"{column.DataType}({(length == -1 ? "MAX" : length.ToString())})";
            }
            else if (dtLowerAdd == "char" || dtLowerAdd == "nchar")
            {
                var length = column.Length ?? 1;
                definition += $"{column.DataType}({length})";
            }
            else if (dtLowerAdd == "decimal" || dtLowerAdd == "numeric")
            {
                var precision = column.Length ?? 18;
                var scale = column.Scale ?? 0;
                definition += $"{column.DataType}({precision}, {scale})";
            }
            else
            {
                definition += column.DataType;
            }

            if (column.IsIdentity)
            {
                definition += " IDENTITY(1,1)";
            }

            if (!column.IsNullable)
            {
                definition += " NOT NULL";
            }

            definition += FormatDefaultForSqlServer(column.DefaultValue);

            var addColumnSql = $"ALTER TABLE [{request.TableName}] ADD {definition}";
            await _context.SqlSugarClient.Ado.ExecuteCommandAsync(addColumnSql);
            // 新列注释
            if (!string.IsNullOrWhiteSpace(column.Comment))
            {
                try
                {
                    var commentSql = $@"
                        EXEC sp_addextendedproperty 
                            @name = N'MS_Description', 
                            @value = N'{column.Comment.Replace("'", "''")}', 
                            @level0type = N'SCHEMA', @level0name = N'dbo', 
                            @level1type = N'TABLE', @level1name = N'{request.TableName.Replace("'", "''")}', 
                            @level2type = N'COLUMN', @level2name = N'{column.ColumnName.Replace("'", "''")}'";
                    await _context.SqlSugarClient.Ado.ExecuteCommandAsync(commentSql);
                }
                catch { /* 注释已存在时忽略 */ }
            }
        }

        // 3) Alter existing columns: 类型/长度/小数位/可空（existingByName 已在 rename 后更新）
        var columnsToAlter = request.Columns
            .Where(c => existingByName.ContainsKey(c.ColumnName))
            .OrderBy(c => c.Order);

        foreach (var column in columnsToAlter)
        {
            var existing = existingByName[column.ColumnName];

            var defaultChanged = (existing.DefaultValue ?? "").Trim() != (column.DefaultValue ?? "").Trim();

            var newTypeRaw = column.DataType?.Trim() ?? "";
            var newTypeLower = newTypeRaw.ToLower();
            var oldTypeLower = (existing.DataType ?? "").Trim().ToLower();

            // 兼容传入 nvarchar(max)/varchar(max)
            if (newTypeLower == "nvarchar(max)") { newTypeLower = "nvarchar"; column.Length = -1; }
            if (newTypeLower == "varchar(max)") { newTypeLower = "varchar"; column.Length = -1; }

            bool typeChanged = !string.Equals(oldTypeLower, newTypeLower, StringComparison.OrdinalIgnoreCase);
            bool nullableChanged = existing.IsNullable != column.IsNullable;

            bool lengthChanged = false;
            bool scaleChanged = false;
            if (newTypeLower is "nvarchar" or "varchar")
            {
                var oldLen = existing.Length ?? 255;
                var newLen = column.Length ?? 255;
                lengthChanged = oldLen != newLen;
            }
            else if (newTypeLower is "char" or "nchar")
            {
                var oldLen = existing.Length ?? 1;
                var newLen = column.Length ?? 1;
                lengthChanged = oldLen != newLen;
            }
            else if (newTypeLower is "decimal" or "numeric")
            {
                var oldPrec = existing.Length ?? 18;
                var newPrec = column.Length ?? 18;
                lengthChanged = oldPrec != newPrec;

                var oldScale = existing.Scale ?? 0;
                var newScale = column.Scale ?? 0;
                scaleChanged = oldScale != newScale;
            }

            bool isIdentityChanged = existing.IsIdentity != column.IsIdentity;

            // 仅当字段有实际修改时才执行更新脚本
            if (!typeChanged && !nullableChanged && !lengthChanged && !scaleChanged && !defaultChanged && !isIdentityChanged)
                continue;

            // IsIdentity 变更：SQL Server 不支持 ALTER COLUMN 修改 IDENTITY，需通过临时表重建
            if (isIdentityChanged)
            {
                await RebuildTableForIdentityChangeAsync(request, existingByName);
                var refreshed = await GetTableInfoAsync(request.TableName);
                foreach (var c in refreshed.Columns)
                    existingByName[c.ColumnName] = c;
                break;
            }

            if (typeChanged || nullableChanged || lengthChanged || scaleChanged)
            {
                string typeSql;
                if (newTypeLower is "nvarchar" or "varchar")
                {
                    var len = column.Length ?? 255;
                    typeSql = $"{newTypeLower}({(len == -1 ? "MAX" : len.ToString())})";
                }
                else if (newTypeLower is "char" or "nchar")
                {
                    var len = column.Length ?? 1;
                    typeSql = $"{newTypeLower}({len})";
                }
                else if (newTypeLower is "decimal" or "numeric")
                {
                    var precision = column.Length ?? 18;
                    var scale = column.Scale ?? 0;
                    typeSql = $"{newTypeLower}({precision}, {scale})";
                }
                else
                {
                    typeSql = newTypeRaw;
                }

                var nullSql = column.IsNullable ? "NULL" : "NOT NULL";
                var alterSql = $"ALTER TABLE [{request.TableName}] ALTER COLUMN [{column.ColumnName}] {typeSql} {nullSql}";
                await _context.SqlSugarClient.Ado.ExecuteCommandAsync(alterSql);
            }

            if (defaultChanged)
            {
                var dfName = $"DF_{request.TableName}_{column.ColumnName}";
                var dropDfSql = $@"DECLARE @cn NVARCHAR(200);
                    SELECT @cn = dc.name FROM sys.default_constraints dc
                    INNER JOIN sys.columns c ON dc.parent_object_id = c.object_id AND dc.parent_column_id = c.column_id
                    WHERE c.object_id = OBJECT_ID('{request.TableName.Replace("'", "''")}') AND c.name = N'{column.ColumnName.Replace("'", "''")}';
                    IF @cn IS NOT NULL EXEC('ALTER TABLE [{request.TableName}] DROP CONSTRAINT [' + @cn + ']')";
                await _context.SqlSugarClient.Ado.ExecuteCommandAsync(dropDfSql);
                if (!string.IsNullOrWhiteSpace(column.DefaultValue))
                {
                    var defVal = column.DefaultValue.Trim();
                    var vLower = defVal.ToLower();
                    var innerExpr = vLower is "getdate()" or "newid()" or "sysdatetime()" ? defVal
                        : long.TryParse(defVal, out _) || decimal.TryParse(defVal, out _) ? defVal
                        : vLower == "true" || vLower == "1" ? "1"
                        : vLower == "false" || vLower == "0" ? "0"
                        : $"N'{defVal.Replace("'", "''")}'";
                    var addDfSql = $"ALTER TABLE [{request.TableName}] ADD CONSTRAINT [{dfName}] DEFAULT ({innerExpr}) FOR [{column.ColumnName}]";
                    await _context.SqlSugarClient.Ado.ExecuteCommandAsync(addDfSql);
                }
            }
        }

        var existingPkCols = existingTable.Columns.Where(c => c.IsPrimaryKey).OrderBy(c => c.Order).Select(c => c.ColumnName).ToList();
        var newPkCols = request.Columns.Where(c => c.IsPrimaryKey).OrderBy(c => c.Order).Select(c => c.ColumnName).ToList();
        var pkChanged = !existingPkCols.SequenceEqual(newPkCols);

        if (pkChanged)
        {
            // Drop existing PK constraint
            var dropPkSql = $@"
                DECLARE @pkConstraintName NVARCHAR(200);
                SELECT @pkConstraintName = name 
                FROM sys.key_constraints 
                WHERE type = 'PK' AND parent_object_id = OBJECT_ID('{request.TableName}');
                IF @pkConstraintName IS NOT NULL
                    EXEC('ALTER TABLE [{request.TableName}] DROP CONSTRAINT [' + @pkConstraintName + ']')";
            await _context.SqlSugarClient.Ado.ExecuteCommandAsync(dropPkSql);

            if (newPkCols.Any())
            {
                var pkConstraintName = $"PK_{request.TableName}";
                var pkColumns = newPkCols.Select(c => $"[{c}]");

                var addPkSql = $"ALTER TABLE [{request.TableName}] ADD CONSTRAINT [{pkConstraintName}] PRIMARY KEY ({string.Join(", ", pkColumns)})";
                await _context.SqlSugarClient.Ado.ExecuteCommandAsync(addPkSql);
            }
        }

        foreach (var column in request.Columns.Where(c => !string.IsNullOrWhiteSpace(c.Comment)))
        {
            if (existingByName.TryGetValue(column.ColumnName, out var ec) && (ec.Comment ?? "").Trim() == (column.Comment ?? "").Trim())
                continue;

            var escapedComment = column.Comment!.Replace("'", "''");
            var tableNameEscaped = request.TableName.Replace("'", "''");
            var columnNameEscaped = column.ColumnName.Replace("'", "''");
            try
            {
                var updateCommentSql = $@"
                    EXEC sp_updateextendedproperty 
                        @name = N'MS_Description', 
                        @value = N'{escapedComment}', 
                        @level0type = N'SCHEMA', @level0name = N'dbo', 
                        @level1type = N'TABLE', @level1name = N'{tableNameEscaped}', 
                        @level2type = N'COLUMN', @level2name = N'{columnNameEscaped}'";
                await _context.SqlSugarClient.Ado.ExecuteCommandAsync(updateCommentSql);
            }
            catch
            {
                try
                {
                    var addCommentSql = $@"
                        EXEC sp_addextendedproperty 
                            @name = N'MS_Description', 
                            @value = N'{escapedComment}', 
                            @level0type = N'SCHEMA', @level0name = N'dbo', 
                            @level1type = N'TABLE', @level1name = N'{tableNameEscaped}', 
                            @level2type = N'COLUMN', @level2name = N'{columnNameEscaped}'";
                    await _context.SqlSugarClient.Ado.ExecuteCommandAsync(addCommentSql);
                }
                catch { /* 忽略 */ }
            }
        }
    }

    public async Task DeleteTableAsync(string tableName)
    {
        var dropTableSql = $"DROP TABLE [{tableName}]";
        await _context.SqlSugarClient.Ado.ExecuteCommandAsync(dropTableSql);
    }
}


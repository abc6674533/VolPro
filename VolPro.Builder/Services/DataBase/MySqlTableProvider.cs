using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using VolPro.Builder.IServices;
using VolPro.Core.EFDbContext;
namespace VolPro.Builder.Services;

/// <summary>
/// MySQL 数据库表结构操作实现。
/// </summary>
public class MySqlTableProvider : ITableDatabaseProvider
{
    private readonly BaseDbContext _context;

    public MySqlTableProvider(BaseDbContext context)
    {
        _context = context;
    }

    public async Task<bool> TableExistsAsync(string tableName)
    {
        const string mysqlSql = @"SELECT COUNT(*) as Value
                FROM INFORMATION_SCHEMA.TABLES
                WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = @tableName;";
        var res = await _context.SqlSugarClient.Ado.GetScalarAsync(mysqlSql, new { tableName });
        return Convert.ToInt32(res) > 0;
    }

    public async Task CreateTableAsync(CreateTableRequest request)
    {
        await CreateTableMySqlAsync(request);
    }

    public async Task<List<string>> GetAllTablesAsync()
    {
        const string mysqlSql = @"
                SELECT TABLE_NAME as Value
                FROM INFORMATION_SCHEMA.TABLES
                WHERE TABLE_SCHEMA = DATABASE()
                ORDER BY TABLE_NAME";
        var rows = await _context.SqlSugarClient.Ado.SqlQueryAsync<string>(mysqlSql);
        return rows ?? new List<string>();
    }

    public async Task<TableInfoDto> GetTableInfoAsync(string tableName)
    {
        return await GetTableInfoMySqlAsync(tableName);
    }

    public async Task UpdateTableAsync(UpdateTableRequest request)
    {
        await UpdateTableMySqlAsync(request);
    }

    public async Task DeleteTableAsync(string tableName)
    {
        var dropMySql = $"DROP TABLE `{tableName.Replace("`", "``")}`;";
        await _context.SqlSugarClient.Ado.ExecuteCommandAsync(dropMySql);
    }

    private static string NormalizeMySqlDefault(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        var s = raw.Trim();
        // MySQL BIT 类型默认值格式为 b'1' 或 b'0'，统一返回 1 或 0
        if (s.StartsWith("b'", StringComparison.OrdinalIgnoreCase) && s.EndsWith("'") && s.Length >= 4)
        {
            var inner = s[2..^1];
            return inner == "1" ? "1" : inner == "0" ? "0" : inner;
        }
        if (s.Length >= 2 && s.StartsWith("'") && s.EndsWith("'"))
            return s[1..^1].Replace("''", "'");
        return s;
    }

    private static string FormatDefaultForMySql(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        var v = value.Trim();
        var vLower = v.ToLower();
        if (vLower is "current_timestamp" or "now()" or "now(6)")
            return $" DEFAULT {v}";
        if (long.TryParse(v, out _) || decimal.TryParse(v, out _))
            return $" DEFAULT {v}";
        if (vLower == "true" || vLower == "1") return " DEFAULT 1";
        if (vLower == "false" || vLower == "0") return " DEFAULT 0";
        return $" DEFAULT '{v.Replace("'", "''")}'";
    }

    // MySQL 类型映射：根据通用 DataType/Length/Scale 生成 MySQL 列类型
    private static string BuildMySqlColumnType(TableColumnDto column)
    {
        var dt = (column.DataType ?? string.Empty).Trim();
        var dtLower = dt.ToLower();

        return dtLower switch
        {
            "nvarchar" or "varchar" or "character varying" => $"VARCHAR({(column.Length is > 0 and <= 65535 ? column.Length : 255)})",
            "char" or "nchar" => $"CHAR({(column.Length is > 0 and <= 255 ? column.Length : 1)})",
            "nvarchar(max)" or "varchar(max)" => "LONGTEXT",
            "int" or "integer" => "INT",
            "bigint" => "BIGINT",
            "tinyint" => "TINYINT",
            "bit" => "BIT",
            "decimal" or "numeric" =>
                $"DECIMAL({(column.Length ?? 18)},{(column.Scale ?? 0)})",
            "double" or "float" => "DOUBLE",
            "datetime" => "DATETIME",
            "date" => "DATE",
            "text" or "ntext" => "TEXT",
            "longtext" => "LONGTEXT",
            "uniqueidentifier" or "uuid" => "CHAR(36)",
            _ => dt // 其他：假设是合法的 MySQL 类型
        };
    }

    // MySQL 专用：建表
    private async Task CreateTableMySqlAsync(CreateTableRequest request)
    {
        var columnDefs = new List<string>();
        foreach (var column in request.Columns.OrderBy(c => c.Order))
        {
            var typeSql = BuildMySqlColumnType(column);
            var isIdentity = column.IsIdentity;
            var nullSql = isIdentity ? "NOT NULL" : (column.IsNullable ? "NULL" : "NOT NULL");
            var identitySql = isIdentity ? " AUTO_INCREMENT" : string.Empty;
            var commentSql = string.IsNullOrWhiteSpace(column.Comment)
                ? string.Empty
                : $" COMMENT '{column.Comment.Replace("'", "''")}'";
            var defaultSql = FormatDefaultForMySql(column.DefaultValue);

            columnDefs.Add($"`{column.ColumnName}` {typeSql}{identitySql} {nullSql}{defaultSql}{commentSql}");
        }

        var pkCols = request.Columns.Where(c => c.IsPrimaryKey).OrderBy(c => c.Order).Select(c => $"`{c.ColumnName}`");
        var pkSql = pkCols.Any() ? $", PRIMARY KEY ({string.Join(", ", pkCols)})" : string.Empty;
        var createSql = $"CREATE TABLE `{request.TableName}` (\n  {string.Join(",\n  ", columnDefs)}{pkSql}\n) ENGINE=InnoDB DEFAULT CHARSET=utf8;";

        await _context.SqlSugarClient.Ado.ExecuteCommandAsync(createSql);
    }

    // MySQL 专用：获取表结构
    private async Task<TableInfoDto> GetTableInfoMySqlAsync(string tableName)
    {
        const string sql = @"
            SELECT 
                c.COLUMN_NAME,
                c.DATA_TYPE,
                c.CHARACTER_MAXIMUM_LENGTH,
                c.IS_NULLABLE,
                c.ORDINAL_POSITION,
                c.NUMERIC_SCALE,
                c.NUMERIC_PRECISION,
                CASE WHEN k.CONSTRAINT_NAME = 'PRIMARY' THEN 1 ELSE 0 END AS IS_PRIMARY_KEY,
                CASE WHEN c.EXTRA LIKE '%auto_increment%' THEN 1 ELSE 0 END AS IS_IDENTITY,
                c.COLUMN_COMMENT,
                c.COLUMN_DEFAULT
            FROM INFORMATION_SCHEMA.COLUMNS c
            LEFT JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE k
                ON k.TABLE_SCHEMA = c.TABLE_SCHEMA
               AND k.TABLE_NAME = c.TABLE_NAME
               AND k.COLUMN_NAME = c.COLUMN_NAME
            WHERE c.TABLE_SCHEMA = DATABASE() AND c.TABLE_NAME = @tableName
            ORDER BY c.ORDINAL_POSITION;";

        var dt = await _context.SqlSugarClient.Ado.GetDataTableAsync(sql, new { tableName });
        var columns = new List<TableColumnDto>();
        foreach (DataRow row in dt.Rows)
        {
            var dataType = row[1]?.ToString() ?? "";
            var isDecimal = dataType.Equals("decimal", StringComparison.OrdinalIgnoreCase);

            int? charLen = null;
            if (!dataType.Equals("text", StringComparison.OrdinalIgnoreCase) &&
                !dataType.Equals("longtext", StringComparison.OrdinalIgnoreCase) &&
                row[2] != DBNull.Value && row[2] != null)
            {
                var raw = Convert.ToInt64(row[2]);
                if (raw > 0 && raw <= int.MaxValue) charLen = (int)raw;
            }

            var numericScale = row[5] == DBNull.Value || row[5] == null ? (int?)null : Convert.ToInt32(row[5]);
            var numericPrecision = row[6] == DBNull.Value || row[6] == null ? (int?)null : Convert.ToInt32(row[6]);
            var colDefault = row[10] == DBNull.Value || row[10] == null ? null : NormalizeMySqlDefault(row[10]?.ToString());

            columns.Add(new TableColumnDto
            {
                ColumnName = row[0]?.ToString() ?? "",
                DataType = dataType,
                Length = isDecimal ? numericPrecision : charLen,
                Scale = isDecimal ? numericScale : null,
                IsNullable = string.Equals(row[3]?.ToString(), "YES", StringComparison.OrdinalIgnoreCase),
                Order = Convert.ToInt32(row[4]),
                IsPrimaryKey = row[7] != DBNull.Value && row[7] != null && Convert.ToInt32(row[7]) == 1,
                IsIdentity = row[8] != DBNull.Value && row[8] != null && Convert.ToInt32(row[8]) == 1,
                Comment = row[9]?.ToString(),
                DefaultValue = colDefault ?? string.Empty
            });
        }

        return new TableInfoDto { TableName = tableName, Columns = columns };
    }

    // MySQL 专用：更新表（仅支持新增列、修改类型/长度/小数位/可空、更新主键）
    private async Task UpdateTableMySqlAsync(UpdateTableRequest request)
    {
        var existing = await GetTableInfoMySqlAsync(request.TableName) ?? throw new InvalidOperationException("Table not found.");
        var existingByName = existing.Columns.ToDictionary(c => c.ColumnName, c => c, StringComparer.Ordinal);
        var newNames = request.Columns.Select(c => c.ColumnName).ToHashSet(StringComparer.Ordinal);

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
            var oldCol = existingByName[actualKey];
            var typeSql = BuildMySqlColumnType(oldCol);
            var isIdentity = oldCol.IsIdentity;
            var nullSql = isIdentity ? "NOT NULL" : (oldCol.IsNullable ? "NULL" : "NOT NULL");
            var identitySql = isIdentity ? " AUTO_INCREMENT" : string.Empty;
            var commentSql = string.IsNullOrWhiteSpace(oldCol.Comment) ? string.Empty : $" COMMENT '{oldCol.Comment.Replace("'", "''")}'";
            var defaultSql = FormatDefaultForMySql(oldCol.DefaultValue);
            var renameSql = $"ALTER TABLE `{request.TableName}` CHANGE COLUMN `{actualKey.Replace("`", "``")}` `{r.ColumnName.Replace("`", "``")}` {typeSql}{identitySql} {nullSql}{defaultSql}{commentSql};";
            await _context.SqlSugarClient.Ado.ExecuteCommandAsync(renameSql);
            existingByName.Remove(actualKey);
            existingByName[r.ColumnName] = new TableColumnDto { ColumnName = r.ColumnName, DataType = oldCol.DataType, Length = oldCol.Length, Scale = oldCol.Scale, IsNullable = oldCol.IsNullable, IsPrimaryKey = oldCol.IsPrimaryKey, IsIdentity = oldCol.IsIdentity, Comment = oldCol.Comment, DefaultValue = oldCol.DefaultValue ?? "", Order = oldCol.Order };
        }

        var existingNames = existingByName.Keys.ToHashSet(StringComparer.Ordinal);
        var columnsToDrop = existingNames.Except(newNames, StringComparer.Ordinal).ToList();

        foreach (var column in request.Columns.Where(c => !existingByName.ContainsKey(c.ColumnName)).OrderBy(c => c.Order))
        {
            var typeSql = BuildMySqlColumnType(column);
            var isIdentity = column.IsIdentity;
            var nullSql = isIdentity ? "NOT NULL" : (column.IsNullable ? "NULL" : "NOT NULL");
            var identitySql = isIdentity ? " AUTO_INCREMENT" : string.Empty;
            var commentSql = string.IsNullOrWhiteSpace(column.Comment) ? string.Empty : $" COMMENT '{column.Comment.Replace("'", "''")}'";
            var defaultSql = FormatDefaultForMySql(column.DefaultValue);
            var addColSql = $"ALTER TABLE `{request.TableName}` ADD COLUMN `{column.ColumnName}` {typeSql}{identitySql} {nullSql}{defaultSql}{commentSql};";
            await _context.SqlSugarClient.Ado.ExecuteCommandAsync(addColSql);
        }

        foreach (var column in request.Columns.Where(c => existingByName.ContainsKey(c.ColumnName)))
        {
            var old = existingByName[column.ColumnName];
            var typeSql = BuildMySqlColumnType(column);
            var oldTypeSql = BuildMySqlColumnType(old);
            var typeChanged = !string.Equals(oldTypeSql, typeSql, StringComparison.OrdinalIgnoreCase);
            var nullableChanged = old.IsNullable != column.IsNullable;
            var defaultChanged = (old.DefaultValue ?? "").Trim() != (column.DefaultValue ?? "").Trim();
            var identityChanged = old.IsIdentity != column.IsIdentity;
            var commentChanged = (old.Comment ?? "").Trim() != (column.Comment ?? "").Trim();

            if (!typeChanged && !nullableChanged && !defaultChanged && !identityChanged && !commentChanged)
                continue;

            var isIdentity = column.IsIdentity;
            var nullSql = isIdentity ? "NOT NULL" : (column.IsNullable ? "NULL" : "NOT NULL");
            var identitySql = isIdentity ? " AUTO_INCREMENT" : string.Empty;
            var commentSql = string.IsNullOrWhiteSpace(column.Comment) ? string.Empty : $" COMMENT '{column.Comment.Replace("'", "''")}'";
            var defaultSql = FormatDefaultForMySql(column.DefaultValue);
            var modifySql = $"ALTER TABLE `{request.TableName}` MODIFY COLUMN `{column.ColumnName}` {typeSql}{identitySql} {nullSql}{defaultSql}{commentSql};";
            await _context.SqlSugarClient.Ado.ExecuteCommandAsync(modifySql);
        }

        foreach (var colName in columnsToDrop)
        {
            await _context.SqlSugarClient.Ado.ExecuteCommandAsync($"ALTER TABLE `{request.TableName}` DROP COLUMN `{colName}`;");
        }

        var newPkCols = request.Columns.Where(c => c.IsPrimaryKey).OrderBy(c => c.Order).Select(c => c.ColumnName).ToList();
        var existingPkCols = existingByName.Values.Where(c => c.IsPrimaryKey).OrderBy(c => c.Order).Select(c => c.ColumnName).ToList();
        var pkChanged = !newPkCols.SequenceEqual(existingPkCols);

        if (pkChanged)
        {
            try { await _context.SqlSugarClient.Ado.ExecuteCommandAsync($"ALTER TABLE `{request.TableName}` DROP PRIMARY KEY;"); } catch { }
            if (newPkCols.Any())
            {
                var pkColsSql = string.Join(", ", newPkCols.Select(c => $"`{c}`"));
                await _context.SqlSugarClient.Ado.ExecuteCommandAsync($"ALTER TABLE `{request.TableName}` ADD PRIMARY KEY ({pkColsSql});");
            }
        }
    }
}


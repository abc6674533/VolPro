using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;
using VolPro.Builder.IServices;
using VolPro.Core.EFDbContext;
namespace VolPro.Builder.Services;

/// <summary>
/// PostgreSQL 数据库表结构操作实现。
/// </summary>
public class PgSqlTableProvider : ITableDatabaseProvider
{
    private readonly BaseDbContext _context;
    private readonly string _schema;

    public PgSqlTableProvider(BaseDbContext context)
    {
        _context = context;

        // 从 PG 连接字符串推断 schema：优先 SearchPath / search_path，其次 schema，默认 public
        var connStr = _context.SqlSugarClient.Ado.Connection?.ConnectionString;
        var schema = "public";
        if (!string.IsNullOrWhiteSpace(connStr))
        {
            try
            {
                var builder = new NpgsqlConnectionStringBuilder(connStr);
                if (!string.IsNullOrWhiteSpace(builder.SearchPath))
                {
                    schema = builder.SearchPath.Split(',')[0].Trim();
                }
                else if (builder.TryGetValue("schema", out var sVal) && sVal is string s && !string.IsNullOrWhiteSpace(s))
                {
                    schema = s.Trim();
                }
            }
            catch
            {
                // 解析失败时回退到 public
                schema = "public";
            }
        }
        _schema = schema;
    }

    public async Task<bool> TableExistsAsync(string tableName)
    {
        const string pgSql = "SELECT COUNT(*) as value FROM information_schema.tables WHERE table_schema = @schemaName AND table_name = @tableName;";
        var res = await _context.SqlSugarClient.Ado.GetScalarAsync(pgSql, new { schemaName = _schema, tableName });
        return Convert.ToInt32(res) > 0;
    }

    public async Task CreateTableAsync(CreateTableRequest request)
    {
        await CreateTablePgSqlAsync(request);
    }

    public async Task<List<string>> GetAllTablesAsync()
    {
        const string pgSql = "SELECT table_name as value FROM information_schema.tables WHERE table_schema = @schemaName ORDER BY table_name;";
        var rows = await _context.SqlSugarClient.Ado.SqlQueryAsync<string>(pgSql, new { schemaName = _schema });
        return rows ?? new List<string>();
    }

    public async Task<TableInfoDto> GetTableInfoAsync(string tableName)
    {
        return await GetTableInfoPgSqlAsync(tableName);
    }

    public async Task UpdateTableAsync(UpdateTableRequest request)
    {
        await UpdateTablePgSqlAsync(request);
    }

    public async Task DeleteTableAsync(string tableName)
    {
        var dropPg = $"DROP TABLE \"{tableName.Replace("\"", "\"\"")}\";";
        await _context.SqlSugarClient.Ado.ExecuteCommandAsync(dropPg);
    }

    private static string NormalizePgSqlDefault(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        var s = raw.Trim();
        if (s.StartsWith("'") && s.Contains("'::"))
        {
            var end = s.IndexOf("'::", StringComparison.Ordinal);
            if (end > 0)
            {
                var inner = s[1..end].Replace("''", "'");
                return inner;
            }
        }
        if (s.StartsWith("nextval(")) return null;
        return s;
    }

    private static string FormatDefaultForPgSql(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        var v = value.Trim();
        var vLower = v.ToLower();
        if (vLower is "current_timestamp" or "now()" or "localtimestamp")
            return $" DEFAULT {v}";
        if (long.TryParse(v, out _) || decimal.TryParse(v, out _))
            return $" DEFAULT {v}";
        if (vLower == "true" || vLower == "1") return " DEFAULT true";
        if (vLower == "false" || vLower == "0") return " DEFAULT false";
        return $" DEFAULT '{v.Replace("'", "''")}'";
    }

    // PGSql 类型映射
    private static string BuildPgSqlColumnType(TableColumnDto column)
    {
        var dt = (column.DataType ?? string.Empty).Trim();
        var dtLower = dt.ToLower();

        return dtLower switch
        {
            "nvarchar" or "varchar" or "character varying" => $"VARCHAR({(column.Length is > 0 ? column.Length : 255)})",
            "char" or "nchar" => $"CHAR({(column.Length is > 0 ? column.Length : 1)})",
            "nvarchar(max)" or "varchar(max)" or "text" or "ntext" => "TEXT",
            "int" or "integer" => "INTEGER",
            "bigint" => "BIGINT",
            "tinyint" or "bit" => "SMALLINT",
            "decimal" or "numeric" =>
                $"NUMERIC({(column.Length ?? 18)},{(column.Scale ?? 0)})",
            "float" or "double" => "DOUBLE PRECISION",
            "datetime" => "TIMESTAMP",
            "date" => "DATE",
            "uniqueidentifier" or "uuid" => "UUID",
            _ => dt
        };
    }

    // PGSql 专用：建表
    private async Task CreateTablePgSqlAsync(CreateTableRequest request)
    {
        var columnDefs = new List<string>();
            foreach (var column in request.Columns.OrderBy(c => c.Order))
            {
                var typeSql = BuildPgSqlColumnType(column);

                var isIntLike = typeSql.StartsWith("INTEGER", StringComparison.OrdinalIgnoreCase) ||
                                typeSql.StartsWith("INT4", StringComparison.OrdinalIgnoreCase) ||
                                typeSql.StartsWith("INT8", StringComparison.OrdinalIgnoreCase) ||
                                typeSql.StartsWith("INT4", StringComparison.OrdinalIgnoreCase) ||
                                typeSql.StartsWith("INT2", StringComparison.OrdinalIgnoreCase) ||
                                typeSql.StartsWith("BIGINT", StringComparison.OrdinalIgnoreCase);
                var wantIdentity = column.IsIdentity && column.IsPrimaryKey && isIntLike;

                // PGSql：自增主键使用 GENERATED BY DEFAULT AS IDENTITY
                var identitySql = wantIdentity ? " GENERATED ALWAYS AS IDENTITY " : string.Empty;

                var nullSql = column.IsNullable && !column.IsPrimaryKey
                    ? "NULL"
                    : "NOT NULL";

                var defaultSql = FormatDefaultForPgSql(column.DefaultValue);

                columnDefs.Add($"\"{column.ColumnName}\" {typeSql}{identitySql} {nullSql}{defaultSql}");
            }

            var pkCols = request.Columns.Where(c => c.IsPrimaryKey)
                .OrderBy(c => c.Order)
                .Select(c => $"\"{c.ColumnName}\"");
            var pkSql = pkCols.Any() ? $", CONSTRAINT \"PK_{request.TableName}\" PRIMARY KEY ({string.Join(", ", pkCols)})" : string.Empty;

            var createSql = $"CREATE TABLE \"{request.TableName}\" (\n  {string.Join(",\n  ", columnDefs)}{pkSql}\n);";

            await _context.SqlSugarClient.Ado.ExecuteCommandAsync(createSql);

            foreach (var column in request.Columns.Where(c => !string.IsNullOrWhiteSpace(c.Comment)))
            {
                var commentSql = $"COMMENT ON COLUMN \"{request.TableName}\".\"{column.ColumnName}\" IS '{column.Comment!.Replace("'", "''")}';";
                await _context.SqlSugarClient.Ado.ExecuteCommandAsync(commentSql);
            }
    }

    // PGSql 专用：获取表结构
    private async Task<TableInfoDto> GetTableInfoPgSqlAsync(string tableName)
    {
        const string sql = @"
            SELECT c.column_name, c.data_type, c.character_maximum_length, c.is_nullable, c.ordinal_position,
                c.numeric_scale, c.numeric_precision,
                CASE WHEN pk.column_name IS NOT NULL THEN 1 ELSE 0 END AS is_primary_key,
                CASE WHEN c.column_default LIKE 'nextval(%' THEN 1 ELSE 0 END AS is_identity,
                col_description(format('%I.%I', c.table_schema, c.table_name)::regclass::oid, a.attnum) AS comment,
                c.column_default
            FROM information_schema.columns c
            LEFT JOIN (SELECT kcu.table_schema, kcu.table_name, kcu.column_name
                FROM information_schema.table_constraints tc
                JOIN information_schema.key_column_usage kcu ON tc.constraint_name = kcu.constraint_name AND tc.table_schema = kcu.table_schema
                WHERE tc.constraint_type = 'PRIMARY KEY') pk
                ON pk.table_schema = c.table_schema AND pk.table_name = c.table_name AND pk.column_name = c.column_name
            JOIN pg_attribute a ON a.attname = c.column_name AND a.attrelid = format('%I.%I', c.table_schema, c.table_name)::regclass
            WHERE c.table_schema = @schemaName AND c.table_name = @tableName ORDER BY c.ordinal_position;";

        var dt = await _context.SqlSugarClient.Ado.GetDataTableAsync(sql, new { schemaName = _schema, tableName });
        var columns = new List<TableColumnDto>();
        foreach (DataRow row in dt.Rows)
        {
            var dataType = row[1]?.ToString() ?? "";
            var isDecimal = dataType.Equals("numeric", StringComparison.OrdinalIgnoreCase) || dataType.Equals("decimal", StringComparison.OrdinalIgnoreCase);

            int? charLen = null;
            if (row[2] != DBNull.Value && row[2] != null)
            {
                var raw = Convert.ToInt64(row[2]);
                if (raw > 0 && raw <= int.MaxValue) charLen = (int)raw;
            }

            var numericScale = row[5] == DBNull.Value || row[5] == null ? (int?)null : Convert.ToInt32(row[5]);
            var numericPrecision = row[6] == DBNull.Value || row[6] == null ? (int?)null : Convert.ToInt32(row[6]);
            var colDefault = row[10] == DBNull.Value || row[10] == null ? null : NormalizePgSqlDefault(row[10]?.ToString());

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

    // PGSql 专用：更新表（新增/删除/修改列 + 主键 & 注释）
    private async Task UpdateTablePgSqlAsync(UpdateTableRequest request)
    {
        var existing = await GetTableInfoPgSqlAsync(request.TableName) ?? throw new InvalidOperationException("Table not found.");
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
                var renameSql = $"ALTER TABLE \"{request.TableName}\" RENAME COLUMN \"{actualKey.Replace("\"", "\"\"")}\" TO \"{r.ColumnName.Replace("\"", "\"\"")}\";";
                await _context.SqlSugarClient.Ado.ExecuteCommandAsync(renameSql);
                var oldCol = existingByName[actualKey];
                existingByName.Remove(actualKey);
                existingByName[r.ColumnName] = new TableColumnDto { ColumnName = r.ColumnName, DataType = oldCol.DataType, Length = oldCol.Length, Scale = oldCol.Scale, IsNullable = oldCol.IsNullable, IsPrimaryKey = oldCol.IsPrimaryKey, IsIdentity = oldCol.IsIdentity, Comment = oldCol.Comment, DefaultValue = oldCol.DefaultValue ?? "", Order = oldCol.Order };
            }

            var existingNames = existingByName.Keys.ToHashSet(StringComparer.Ordinal);
            var columnsToDrop = existingNames.Except(newNames, StringComparer.Ordinal).ToList();

            // 2) 新增列
            foreach (var column in request.Columns.Where(c => !existingByName.ContainsKey(c.ColumnName)).OrderBy(c => c.Order))
            {
                var typeSql = BuildPgSqlColumnType(column);
                var isIdentity = column.IsIdentity &&
                                 (typeSql.StartsWith("INTEGER", StringComparison.OrdinalIgnoreCase) ||
                                  typeSql.StartsWith("BIGINT", StringComparison.OrdinalIgnoreCase));
                var identitySql = isIdentity ? " GENERATED ALWAYS AS IDENTITY" : string.Empty;
                var nullSql = column.IsNullable && !column.IsPrimaryKey
                    ? "NULL"
                    : "NOT NULL";

                var defaultSql = FormatDefaultForPgSql(column.DefaultValue);

                var addColSql = $"ALTER TABLE \"{request.TableName}\" ADD COLUMN \"{column.ColumnName}\" {typeSql}{identitySql} {nullSql}{defaultSql};";
                await _context.SqlSugarClient.Ado.ExecuteCommandAsync(addColSql);

                if (!string.IsNullOrWhiteSpace(column.Comment))
                {
                    var commentSql = $"COMMENT ON COLUMN \"{request.TableName}\".\"{column.ColumnName}\" IS '{column.Comment!.Replace("'", "''")}';";
                    await _context.SqlSugarClient.Ado.ExecuteCommandAsync(commentSql);
                }
            }
             
            // 3) 修改已有列：仅当有变更时才执行 ALTER
            foreach (var column in request.Columns.Where(c => existingByName.ContainsKey(c.ColumnName)))
            {
                var typeSql = BuildPgSqlColumnType(column);
                var old = existingByName[column.ColumnName];
                var isIntegerLike = typeSql.StartsWith("INTEGER", StringComparison.OrdinalIgnoreCase) ||
                                    typeSql.StartsWith("BIGINT", StringComparison.OrdinalIgnoreCase);
                var wantIdentity = column.IsIdentity && column.IsPrimaryKey && isIntegerLike;

                var oldTypeSql = BuildPgSqlColumnType(old);
                var typeChanged = !string.Equals(oldTypeSql, typeSql, StringComparison.OrdinalIgnoreCase);
                var nullableChanged = old.IsNullable != column.IsNullable;
                var defaultChanged = (old.DefaultValue ?? "").Trim() != (column.DefaultValue ?? "").Trim();
                var identityChanged = isIntegerLike && old.IsIdentity != wantIdentity;
                var commentChanged = (old.Comment ?? "").Trim() != (column.Comment ?? "").Trim();

                if (typeChanged)
                {
                    var alterTypeSql = $"ALTER TABLE \"{request.TableName}\" ALTER COLUMN \"{column.ColumnName}\" TYPE {typeSql};";
                    await _context.SqlSugarClient.Ado.ExecuteCommandAsync(alterTypeSql);
                }

                if (nullableChanged)
                {
                    var nullSql = column.IsNullable && !column.IsPrimaryKey ? "DROP NOT NULL" : "SET NOT NULL";
                    await _context.SqlSugarClient.Ado.ExecuteCommandAsync($"ALTER TABLE \"{request.TableName}\" ALTER COLUMN \"{column.ColumnName}\" {nullSql};");
                }

                if (defaultChanged)
                {
                    var defaultSql = FormatDefaultForPgSql(column.DefaultValue);
                    if (!string.IsNullOrWhiteSpace(defaultSql))
                        await _context.SqlSugarClient.Ado.ExecuteCommandAsync($"ALTER TABLE \"{request.TableName}\" ALTER COLUMN \"{column.ColumnName}\" SET {defaultSql.Trim()};");
                    else
                        try { await _context.SqlSugarClient.Ado.ExecuteCommandAsync($"ALTER TABLE \"{request.TableName}\" ALTER COLUMN \"{column.ColumnName}\" DROP DEFAULT;"); } catch { }
                }

                if (identityChanged)
                {
                    if (wantIdentity && !old.IsIdentity)
                        await _context.SqlSugarClient.Ado.ExecuteCommandAsync($"ALTER TABLE \"{request.TableName}\" ALTER COLUMN \"{column.ColumnName}\" ADD GENERATED BY DEFAULT AS IDENTITY;");
                    else if (!wantIdentity && old.IsIdentity)
                        await _context.SqlSugarClient.Ado.ExecuteCommandAsync($"ALTER TABLE \"{request.TableName}\" ALTER COLUMN \"{column.ColumnName}\" DROP IDENTITY IF EXISTS;");
                }

                if (commentChanged)
                {
                    var commentText = string.IsNullOrWhiteSpace(column.Comment) ? "NULL" : $"'{column.Comment!.Replace("'", "''")}'";
                    await _context.SqlSugarClient.Ado.ExecuteCommandAsync($"COMMENT ON COLUMN \"{request.TableName}\".\"{column.ColumnName}\" IS {commentText};");
                }
            }

            foreach (var colName in columnsToDrop)
            {
                await _context.SqlSugarClient.Ado.ExecuteCommandAsync($"ALTER TABLE \"{request.TableName}\" DROP COLUMN \"{colName}\";");
            }

            var newPkCols = request.Columns.Where(c => c.IsPrimaryKey).OrderBy(c => c.Order).Select(c => c.ColumnName).ToList();
            var existingPkCols = existingByName.Values.Where(c => c.IsPrimaryKey).OrderBy(c => c.Order).Select(c => c.ColumnName).ToList();
            var pkChanged = !newPkCols.SequenceEqual(existingPkCols);

            if (pkChanged)
            {
                const string findPkSql = "SELECT c.conname FROM pg_constraint c JOIN pg_class t ON c.conrelid = t.oid JOIN pg_namespace n ON n.oid = t.relnamespace WHERE c.contype = 'p' AND n.nspname = @schemaName AND t.relname = @tableName;";
                var pkDt = await _context.SqlSugarClient.Ado.GetDataTableAsync(findPkSql, new { schemaName = _schema, tableName = request.TableName });
                foreach (DataRow r in pkDt.Rows)
                {
                    var pkName = r[0]?.ToString();
                    if (!string.IsNullOrEmpty(pkName))
                        await _context.SqlSugarClient.Ado.ExecuteCommandAsync($"ALTER TABLE \"{_schema}\".\"{request.TableName}\" DROP CONSTRAINT \"{pkName}\";");
                }

                if (newPkCols.Any())
                {
                    var pkColsSql = string.Join(", ", newPkCols.Select(c => $"\"{c}\""));
                    await _context.SqlSugarClient.Ado.ExecuteCommandAsync($"ALTER TABLE \"{_schema}\".\"{request.TableName}\" ADD CONSTRAINT \"PK_{request.TableName}\" PRIMARY KEY ({pkColsSql});");
                }
            }
    }
}


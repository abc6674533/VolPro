using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using VolPro.Core.Const;
using VolPro.Core.DBManager;
using VolPro.Core.EFDbContext;
using VolPro.Core.Enums;
using VolPro.Core.Utilities;

namespace VolPro.Builder.Services
{
    /// <summary>
    /// SQL 表结构信息
    /// </summary>
    public class SqlColumnSchemaInfo
    {
        /// <summary>字段名</summary>
        public string ColumnName { get; set; } = string.Empty;

        /// <summary>备注（说明）</summary>
        /// <remarks>对任意 SQL 结果集，通常无法获取扩展属性/注释，此处可能为空</remarks>
        public string Comment { get; set; } = string.Empty;

        /// <summary>长度/精度</summary>
        public int? Length { get; set; }

        /// <summary>C# 类型（如 String、Int32、Decimal 等）</summary>
        public string DataType { get; set; } = string.Empty;

        /// <summary>数据库字段类型（如 nvarchar、int、decimal 等）</summary>
        public string DbDataType { get; set; } = string.Empty;

        /// <summary>小数位数（decimal/numeric 等）</summary>
        public int? Scale { get; set; }

        /// <summary>是否可为空</summary>
        public bool IsNullable { get; set; } = true;

        /// <summary>列序号</summary>
        public int Ordinal { get; set; }
    }
    public static class SqlSchemaHelper
    {
        public static (bool IsValid, string ErrorMessage) IsValidSql(string sql, string dbServer = "SysDbContext")
        {
            if (string.IsNullOrWhiteSpace(sql))
                return (true, string.Empty);
            var connection = GetDbConnection(dbServer);
            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
            }
            try
            {
                using var cmd = connection.CreateCommand();
                cmd.CommandText = sql;
                using var reader = cmd.ExecuteReader(CommandBehavior.SchemaOnly);
                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, $"sql执行未通过,请检查sql,{ex.InnerException?.Message ?? ex.Message}");
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }
        private static IDbConnection GetDbConnection(string dbServer = "SysDbContext")
        {
            var type = DbRelativeCache.GetDbContextType(dbServer);
            object dbObj = HttpContext.Current.RequestServices.GetService(type);
            BaseDbContext context = (BaseDbContext)dbObj;
            var connection = context.SqlSugarClient.Ado.Connection;
            return connection;
        }
        public static List<SqlColumnSchemaInfo> GetSchemaFromSql(string sql, string dbServer = "SysDbContext")
        {
            List<SqlColumnSchemaInfo> list = new List<SqlColumnSchemaInfo>();
            var connection = GetDbConnection(dbServer);
            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
            }
            try
            {
                using var cmd = connection.CreateCommand();
                cmd.CommandText = sql;
                using var reader = cmd.ExecuteReader(CommandBehavior.SchemaOnly);
                if (reader == null) return list;

                var schemaTable = reader.GetSchemaTable();
                if (schemaTable == null || schemaTable.Rows.Count == 0)
                    return list;

                int ordinal = 0;
                foreach (DataRow row in schemaTable.Rows)
                {
                    var info = RowToColumnInfo(row, reader, ordinal++);
                    if (info != null)
                        list.Add(info);
                }
                return list;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }

        private static SqlColumnSchemaInfo RowToColumnInfo(DataRow row, IDataReader reader, int ordinal)
        {
            var colName = GetRowValue<string>(row, "ColumnName")
                ?? GetRowValue<string>(row, "BaseColumnName");

            var dataType = GetRowValue<Type>(row, "DataType");
            var netTypeName = dataType?.Name ?? GetRowValue<string>(row, "DataTypeName") ?? string.Empty;

            var dbDataType = string.Empty;
            if (reader != null && ordinal >= 0 && ordinal < reader.FieldCount)
            {
                try { dbDataType = reader.GetDataTypeName(ordinal) ?? string.Empty; }
                catch { }
            }
            if (string.IsNullOrEmpty(dbDataType))
                dbDataType = GetRowValue<string>(row, "DataTypeName") ?? string.Empty;

            var length = GetRowValue<int?>(row, "ColumnSize");
            var precision = GetRowValue<int?>(row, "NumericPrecision");
            var scale = GetRowValue<int?>(row, "NumericScale");

            if (length.HasValue && length.Value <= 0)
                length = null;
            if (dataType != null && (netTypeName.Contains("Decimal", StringComparison.OrdinalIgnoreCase) || netTypeName.Contains("Numeric", StringComparison.OrdinalIgnoreCase)))
            {
                length = precision;
            }

            var allowNull = true;
            if (row.Table.Columns.Contains("AllowDBNull"))
            {
                var allowNullVal = row["AllowDBNull"];
                if (allowNullVal != null && allowNullVal != DBNull.Value)
                    allowNull = Convert.ToBoolean(allowNullVal);
            }

            var csharpTypeName = DbTypeToCSharpType(dbDataType, dataType, netTypeName);

            return new SqlColumnSchemaInfo
            {
                ColumnName = colName ?? string.Empty,
                Comment = string.Empty,
                Length = length,
                Scale = scale,
                DataType = csharpTypeName,
                DbDataType = dbDataType,
                IsNullable = allowNull,
                Ordinal = ordinal
            };
        }

        /// <summary>
        /// 将数据库字段类型转换为 C# 类型名称（如 int、string、DateTime、decimal 等）
        /// 参照 Sys_TableInfoService 的 GetSqlServerStructure/GetMySqlStructure/GetPgSqlStructure 映射
        /// </summary>
        private static string DbTypeToCSharpType(string dbDataType, Type netType, string netTypeName)
        {
            var dbLower = (dbDataType ?? string.Empty).Trim().ToLowerInvariant();
            if (!string.IsNullOrEmpty(dbLower))
            {
                // SQL Server、MySQL、PostgreSQL 等统一映射
                switch (dbLower)
                {
                    case "uniqueidentifier":
                    case "uuid":
                        return "guid";
                    case "bit":
                    case "bool":
                    case "boolean":
                        return "bool";
                    case "tinyint":
                        return "byte";
                    case "smallint":
                    case "int2":
                        return "short";
                    case "int":
                    case "integer":
                    case "mediumint":
                    case "int4":
                    case "year":
                        return "int";
                    case "bigint":
                    case "int8":
                        return "long";
                    case "real":
                    case "float4":
                        return "float";
                    case "float":
                    case "double":
                    case "float8":
                        return "double";
                    case "decimal":
                    case "numeric":
                    case "money":
                    case "smallmoney":
                        return "decimal";
                    case "time":
                        return "TimeSpan";
                    case "date":
                    case "datetime":
                    case "datetime2":
                    case "smalldatetime":
                    case "timestamp":
                        return "DateTime";
                    case "char":
                    case "varchar":
                    case "nvarchar":
                    case "nchar":
                    case "text":
                    case "ntext":
                    case "xml":
                    case "varbinary":
                    case "image":
                    case "tinytext":
                    case "mediumtext":
                    case "longtext":
                    case "tinyblob":
                    case "blob":
                    case "mediumblob":
                    case "longblob":
                    case "bytea":
                        return "string";
                }
            }

            // 无 DbDataType 或未匹配时，按 .NET Type 映射
            if (netType != null)
            {
                if (netType == typeof(bool) || netType == typeof(bool?)) return "bool";
                if (netType == typeof(byte) || netType == typeof(byte?)) return "byte";
                if (netType == typeof(short) || netType == typeof(short?)) return "short";
                if (netType == typeof(int) || netType == typeof(int?)) return "int";
                if (netType == typeof(long) || netType == typeof(long?)) return "long";
                if (netType == typeof(float) || netType == typeof(float?)) return "float";
                if (netType == typeof(double) || netType == typeof(double?)) return "double";
                if (netType == typeof(decimal) || netType == typeof(decimal?)) return "decimal";
                if (netType == typeof(DateTime) || netType == typeof(DateTime?)) return "DateTime";
                if (netType == typeof(TimeSpan) || netType == typeof(TimeSpan?)) return "TimeSpan";
                if (netType == typeof(Guid) || netType == typeof(Guid?)) return "guid";
                if (netType == typeof(string)) return "string";
                if (netType == typeof(byte[])) return "byte[]";
            }

            if (!string.IsNullOrEmpty(netTypeName))
            {
                if (netTypeName.IndexOf("Boolean", StringComparison.OrdinalIgnoreCase) >= 0) return "bool";
                if (netTypeName.IndexOf("Byte", StringComparison.OrdinalIgnoreCase) >= 0) return "byte";
                if (netTypeName.IndexOf("Int16", StringComparison.OrdinalIgnoreCase) >= 0) return "short";
                if (netTypeName.IndexOf("Int32", StringComparison.OrdinalIgnoreCase) >= 0) return "int";
                if (netTypeName.IndexOf("Int64", StringComparison.OrdinalIgnoreCase) >= 0) return "long";
                if (netTypeName.IndexOf("Single", StringComparison.OrdinalIgnoreCase) >= 0) return "float";
                if (netTypeName.IndexOf("Double", StringComparison.OrdinalIgnoreCase) >= 0) return "double";
                if (netTypeName.IndexOf("Decimal", StringComparison.OrdinalIgnoreCase) >= 0 || netTypeName.IndexOf("Numeric", StringComparison.OrdinalIgnoreCase) >= 0) return "decimal";
                if (netTypeName.IndexOf("DateTime", StringComparison.OrdinalIgnoreCase) >= 0) return "DateTime";
                if (netTypeName.IndexOf("TimeSpan", StringComparison.OrdinalIgnoreCase) >= 0) return "TimeSpan";
                if (netTypeName.IndexOf("Guid", StringComparison.OrdinalIgnoreCase) >= 0) return "guid";
                if (netTypeName.IndexOf("String", StringComparison.OrdinalIgnoreCase) >= 0) return "string";
            }

            return "string";
        }

        private static T GetRowValue<T>(DataRow row, string columnName)
        {
            if (!row.Table.Columns.Contains(columnName))
                return default;

            var val = row[columnName];
            if (val == null || val == DBNull.Value)
                return default;

            var targetType = typeof(T);
            if (targetType == typeof(string))
                return (T)(object)(val?.ToString() ?? string.Empty);
            if (targetType == typeof(Type))
                return val is Type t ? (T)(object)t : default;
            if (targetType == typeof(int?) || targetType == typeof(int))
            {
                try { return (T)(object)Convert.ToInt32(val); }
                catch { return default; }
            }

            try
            {
                var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;
                return (T)Convert.ChangeType(val, underlyingType);
            }
            catch
            {
                return default;
            }
        }
    }
}

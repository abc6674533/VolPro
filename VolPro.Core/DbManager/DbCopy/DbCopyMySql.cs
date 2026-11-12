using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VolPro.Core.Extensions;
using VolPro.Core.Services;
using VolPro.Core.Utilities;

namespace VolPro.Core.DbManager.DbCopy
{

    /// <summary>
    /// MySQL 数据库复制工具类
    /// </summary>
    public class DbCopyMysql
    {

        public static WebResponseContent CopyDatabase(ISqlSugarClient db,ISqlSugarClient toDbContext, string sourceDatabase,
            string targetDatabase, bool includeData = true)
        {
            var result = new CopyResult();
            var logs = result.Logs;
          
            try
            {
                // 验证
                if (string.IsNullOrWhiteSpace(sourceDatabase))
                    throw new ArgumentException("源数据库名不能为空");
                if (string.IsNullOrWhiteSpace(targetDatabase))
                    throw new ArgumentException("目标数据库名不能为空");
                if (sourceDatabase.Equals(targetDatabase, StringComparison.OrdinalIgnoreCase))
                    throw new ArgumentException("源和目标数据库不能相同");

                logs.Add($"[{DateTime.Now:HH:mm:ss}] 开始复制: {sourceDatabase} -> {targetDatabase}");

                // 检查源数据库
                if (!DbExists(db, sourceDatabase))
                    throw new Exception($"源数据库 '{sourceDatabase}' 不存在");

                // 检查目标数据库
                if (DbExists(db, targetDatabase))
                    throw new Exception($"目标数据库 '{targetDatabase}' 已存在");

                // 创建目标数据库
                logs.Add($"[{DateTime.Now:HH:mm:ss}] 创建目标数据库...");
                CreateDb(db, sourceDatabase, targetDatabase);

                // 复制表
                logs.Add($"[{DateTime.Now:HH:mm:ss}] 复制表...");
                CopyTables(db,toDbContext, sourceDatabase, targetDatabase, includeData, result, logs);

                // 复制视图
                logs.Add($"[{DateTime.Now:HH:mm:ss}] 复制视图...");
                result.ViewsCount = CopyViews(db,toDbContext, sourceDatabase, targetDatabase, logs);

                // 复制存储过程
                logs.Add($"[{DateTime.Now:HH:mm:ss}] 复制存储过程...");
                result.ProceduresCount = CopyRoutines(db, toDbContext, sourceDatabase, targetDatabase, "PROCEDURE", logs);

                // 复制函数
                logs.Add($"[{DateTime.Now:HH:mm:ss}] 复制函数...");
                result.FunctionsCount = CopyRoutines(db,toDbContext, sourceDatabase, targetDatabase, "FUNCTION", logs);

                // 复制触发器
                logs.Add($"[{DateTime.Now:HH:mm:ss}] 复制触发器...");
                result.TriggersCount = CopyTriggers(db, sourceDatabase, targetDatabase, logs);

                result.Success = true;
                result.Message = "复制成功";

                logs.Add($"[{DateTime.Now:HH:mm:ss}] ✓ 完成! 表:{result.TablesCount} 视图:{result.ViewsCount} " +
                         $"存储过程:{result.ProceduresCount} 函数:{result.FunctionsCount} 触发器:{result.TriggersCount} " +
                         $"行数:{result.TotalRowsCopied} 耗时:{result.ElapsedSeconds:F2}秒");
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
                logs.Add($"[{DateTime.Now:HH:mm:ss}] ✗ 错误: {ex.Message}");
            }
            WebResponseContent webResponse = new WebResponseContent();
            webResponse.Status = result.Success;
            webResponse.Message = result.Message;
            Logger.Info($"mysql创建数据库：{logs.Serialize()}");
            return webResponse;
        }

        /// <summary>
        /// 获取数据库列表
        /// </summary>
        public static List<string> GetDatabases(ISqlSugarClient db)
        {
            var all = db.Ado.SqlQuery<string>("SHOW DATABASES");
            var system = new[] { "information_schema", "mysql", "performance_schema", "sys" };
            return all.Where(x => !system.Contains(x.ToLower())).ToList();
        }

        private static bool DbExists(ISqlSugarClient db, string name)
        {
            return db.Ado.GetInt(
                "SELECT COUNT(*) FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = @name",
                new { name }) > 0;
        }

        private static void CreateDb(ISqlSugarClient db, string source, string target)
        {
            var info = db.Ado.SqlQuerySingle<dynamic>(@"
            SELECT DEFAULT_CHARACTER_SET_NAME AS Charset, DEFAULT_COLLATION_NAME AS Collation
            FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = @name", new { name = source });

            var charset = info?.Charset ?? "utf8mb4";
            var collation = info?.Collation ?? "utf8mb4_general_ci";

            db.Ado.ExecuteCommand($"CREATE DATABASE `{target}` CHARACTER SET {charset} COLLATE {collation}");
        }

        private static void CopyTables(ISqlSugarClient db, ISqlSugarClient toDbContext, string source, string target,
            bool includeData, CopyResult result, List<string> logs)
        {
            //db.Ado.ExecuteCommand($"USE `{source}`");
            var tables = db.Ado.SqlQuery<string>($"USE `{source}`;SHOW TABLES;");

            db.Ado.ExecuteCommand($"USE `{target}`");
            db.Ado.ExecuteCommand("SET FOREIGN_KEY_CHECKS = 0");

            foreach (var table in tables)
            {
                try
                {
                    // 获取建表语句
                    //db.Ado.ExecuteCommand($"USE `{source}`");
                    var row = db.Ado.SqlQuerySingle<dynamic>($"USE `{source}`;SHOW CREATE TABLE `{table}`");
                    var sql = GetValue(row, "Create Table");
                    if (string.IsNullOrEmpty(sql)) continue;

                    //创建表
                    toDbContext.Ado.ExecuteCommand($"USE `{target}`");
                    toDbContext.Ado.ExecuteCommand(sql);
                    result.TablesCount++;
                    logs.Add($"  ✓ {table}");

                    // 复制数据
                    if (includeData)
                    {
                        var rows = db.Ado.ExecuteCommand(
                            $"INSERT INTO `{target}`.`{table}` SELECT * FROM `{source}`.`{table}`");
                        result.TotalRowsCopied += rows;
                        if (rows > 0) logs.Add($"    {rows} 行");
                    }
                }
                catch (Exception ex)
                {
                    logs.Add($"  ✗ {table}: {ex.Message}");
                }
            }

            db.Ado.ExecuteCommand("SET FOREIGN_KEY_CHECKS = 1");
        }

        private static int CopyViews(ISqlSugarClient db, ISqlSugarClient toDbContext, string source, string target, List<string> logs)
        {
            var views = db.Ado.SqlQuery<string>(
                "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.VIEWS WHERE TABLE_SCHEMA = @db",
                new { db = source });

            var count = 0;
            foreach (var view in views)
            {
                try
                {
                    //db.Ado.ExecuteCommand($"USE `{source}`");
                     var row = db.Ado.SqlQuerySingle<dynamic>($"USE `{source}`;SHOW CREATE VIEW `{view}`;");
                    //var row = db.Ado.SqlQuerySingle<dynamic>($"SHOW CREATE VIEW `{view}`;");
                    var sql = GetValue(row, "Create View");
                    if (string.IsNullOrEmpty(sql)) continue;

                    sql = sql.Replace($"`{source}`.", $"`{target}`.");
                    sql = Regex.Replace(sql, @"DEFINER=`[^`]+`@`[^`]+`\s*", "");

                    toDbContext.Ado.ExecuteCommand($"USE `{target}`");
                    toDbContext.Ado.ExecuteCommand(sql);
                    count++;
                    logs.Add($"  ✓ {view}");
                }
                catch (Exception ex)
                {
                    logs.Add($"  ✗ {view}: {ex.Message}");
                }
            }
            return count;
        }

        private static int CopyRoutines(ISqlSugarClient db, ISqlSugarClient toDbContext, string source, string target,
            string type, List<string> logs)
        {
            var routines = db.Ado.SqlQuery<string>(
                "SELECT ROUTINE_NAME FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = @db AND ROUTINE_TYPE = @type",
                new { db = source, type });

            var count = 0;
            var key = type == "PROCEDURE" ? "Create Procedure" : "Create Function";

            foreach (var routine in routines)
            {
                try
                {
                    //db.Ado.ExecuteCommand($"USE `{source}`");
                    var row = db.Ado.SqlQuerySingle<dynamic>($"USE `{source}`;SHOW CREATE {type} `{routine}`;");
                   // var row = db.Ado.SqlQuerySingle<dynamic>($"SHOW CREATE {type} `{routine}`;");
                    var sql = GetValue(row, key);
                    if (string.IsNullOrEmpty(sql)) continue;

                    sql = Regex.Replace(sql, @"DEFINER=`[^`]+`@`[^`]+`\s*", "");

                    toDbContext.Ado.ExecuteCommand($"USE `{target}`");
                    toDbContext.Ado.ExecuteCommand(sql);
                    count++;
                    logs.Add($"  ✓ {routine}");
                }
                catch (Exception ex)
                {
                    logs.Add($"  ✗ {routine}: {ex.Message}");
                }
            }
            return count;
        }

        private static int CopyTriggers(ISqlSugarClient db, string source, string target, List<string> logs)
        {
            var triggers = db.Ado.SqlQuery<string>(
                "SELECT TRIGGER_NAME FROM INFORMATION_SCHEMA.TRIGGERS WHERE TRIGGER_SCHEMA = @db",
                new { db = source });

            var count = 0;
            foreach (var trigger in triggers)
            {
                try
                {
                    //db.Ado.ExecuteCommand($"USE `{source}`");
                    var row = db.Ado.SqlQuerySingle<dynamic>($"USE `{source}`;SHOW CREATE TRIGGER `{trigger}`;");
                    //var row = db.Ado.SqlQuerySingle<dynamic>($"SHOW CREATE TRIGGER `{trigger}`;");
                    var sql = GetValue(row, "SQL Original Statement");
                    if (string.IsNullOrEmpty(sql)) continue;

                    sql = Regex.Replace(sql, @"DEFINER=`[^`]+`@`[^`]+`\s*", "");

                    db.Ado.ExecuteCommand($"USE `{target}`");
                    db.Ado.ExecuteCommand(sql);
                    count++;
                    logs.Add($"  ✓ {trigger}");
                }
                catch (Exception ex)
                {
                    logs.Add($"  ✗ {trigger}: {ex.Message}");
                }
            }
            return count;
        }

        private static string GetValue(dynamic row, string key)
        {
            if (row == null) return "";
            var dict = row as IDictionary<string, object>;
            return dict?.ContainsKey(key) == true ? dict[key]?.ToString() ?? "" : "";
        }
    }
 
    public class CopyResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public int TablesCount { get; set; }
        public int ViewsCount { get; set; }
        public int ProceduresCount { get; set; }
        public int FunctionsCount { get; set; }
        public int TriggersCount { get; set; }
        public long TotalRowsCopied { get; set; }
        public double ElapsedSeconds { get; set; }
        public List<string> Logs { get; set; } = new();
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using VolPro.Builder.Services;
using VolPro.Core.Utilities;

namespace VolPro.Builder.IServices;

public interface ITableService
{
    /// <summary>
    /// 判断指定表是否存在。
    /// </summary>
    /// <param name="dbService">数据库类型标识，例如 SqlServer / MySql / PGSql，如果为 null 则使用默认配置。</param>
    Task<bool> TableExistsAsync(string dbService, string tableName);

    /// <summary>
    /// 创建表。
    /// </summary>
    /// <param name="dbService">数据库类型标识。</param>
    Task<WebResponseContent> CreateTableAsync(string dbService, CreateTableRequest request);

    /// <summary>
    /// 获取所有表名。
    /// </summary>
    /// <param name="dbService">数据库类型标识。</param>
    Task<object> GetAllTablesAsync(string dbService);

    /// <summary>
    /// 获取表结构信息。
    /// </summary>
    /// <param name="dbService">数据库类型标识。</param>
    Task<TableInfoDto> GetTableInfoAsync(string dbService, string tableName);

    /// <summary>
    /// 更新表结构。
    /// </summary>
    /// <param name="dbService">数据库类型标识。</param>
    Task<WebResponseContent> UpdateTableAsync(string dbService, UpdateTableRequest request);

    /// <summary>
    /// 删除表。
    /// </summary>
    /// <param name="dbService">数据库类型标识。</param>
    Task<WebResponseContent> DeleteTableAsync(string dbService, string tableName);
}
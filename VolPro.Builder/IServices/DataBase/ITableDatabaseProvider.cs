using System.Collections.Generic;
using System.Threading.Tasks;
using VolPro.Builder.Services;

namespace VolPro.Builder.IServices;

/// <summary>
/// 按数据库类型实现的表结构操作策略接口。
/// </summary>
public interface ITableDatabaseProvider
{
    Task<bool> TableExistsAsync(string tableName);

    Task CreateTableAsync(CreateTableRequest request);

    Task<List<string>> GetAllTablesAsync();

    Task<TableInfoDto> GetTableInfoAsync(string tableName);

    Task UpdateTableAsync(UpdateTableRequest request);

    Task DeleteTableAsync(string tableName);
}


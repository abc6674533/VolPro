using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using VolPro.Builder.IServices;
using VolPro.Builder.Services;
using VolPro.Core;
using VolPro.Core.Extensions.AutofacManager;
using VolPro.Core.UserManager;
using VolPro.Core.Utilities;

namespace VolPro.Builder.Services;

public class TableService : ITableService, IDependency
{
	private readonly ITableProviderFactory _factory;
    WebResponseContent webResponse = new WebResponseContent();
    public TableService(ITableProviderFactory factory)
	{
		_factory = factory;
	} 

	public async Task<bool> TableExistsAsync(string dbService, string tableName)
	{
		var provider = _factory.GetProvider(dbService);
		return await provider.TableExistsAsync(tableName);
	}

	public async Task<WebResponseContent> CreateTableAsync(string dbService, CreateTableRequest request)
	{
		var provider = _factory.GetProvider(dbService);

		if (await provider.TableExistsAsync(request.TableName))
		{
            return webResponse.Error($"{request.TableName}表已存在");
        }
		await provider.CreateTableAsync(request);
		return webResponse.OK("创建成功");
	}
	public async Task<object> GetAllTablesAsync(string dbService)
	{
		var provider = _factory.GetProvider(dbService);
		var tables = await provider.GetAllTablesAsync();
		var tableInfo = TableColumnContext.TableInfo;
		var list = tables.Select(s => new
		{
			table = s,
			name = tableInfo.Where(x => x.TableName == s).Select(x => x.ColumnCNName).FirstOrDefault() ?? s
		}).OrderBy(item => !IsStartWithLetter(item.name))
		  .ThenBy(item => item.name, StringComparer.OrdinalIgnoreCase)
		  .ToList();
		return list;
	}
	private static bool IsStartWithLetter(string str)
	{
		if (string.IsNullOrEmpty(str)) return false;
		char firstChar = str[0];
		return (firstChar >= 'a' && firstChar <= 'z') ||
			   (firstChar >= 'A' && firstChar <= 'Z');
	}

	public async Task<TableInfoDto> GetTableInfoAsync(string dbService, string tableName)
	{
		var provider = _factory.GetProvider(dbService);

		var res= await provider.GetTableInfoAsync(tableName);
        foreach (var item in res.Columns)
        {
			item.IsDbField = true;
			item.OriginalColumnName = item.ColumnName ?? string.Empty;
			item.Comment = TableColumnContext.Data.Where(x => x.ColumnName == item.ColumnName).Select(s => s.ColumnCnName).FirstOrDefault() ?? item.Comment;
        }

        return res;
	}

	public async Task<WebResponseContent> UpdateTableAsync(string dbService, UpdateTableRequest request)
	{
		var provider = _factory.GetProvider(dbService);
        if (!await provider.TableExistsAsync(request.TableName))
        {
            return webResponse.Error($"{request.TableName}不存在");
        }
        await provider.UpdateTableAsync(request);
		return webResponse.OK("修改成功");
	}

	public async Task<WebResponseContent> DeleteTableAsync(string dbService, string tableName)
	{
	
		if (tableName.ToLower().StartsWith("sys"))
		{
			return webResponse.Error("系统表不能删除,请在数据库操作".Translator());
		}
		var provider = _factory.GetProvider(dbService);

		if (!await provider.TableExistsAsync(tableName)) {
			return webResponse.Error($"{tableName}不存在");
		}
		await provider.DeleteTableAsync(tableName);
		return webResponse.OK("删除成功");
	}

}
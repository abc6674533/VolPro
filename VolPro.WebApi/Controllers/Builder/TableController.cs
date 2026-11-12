using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VolPro.Builder.IServices;
using VolPro.Builder.Services;
using VolPro.Core.Configuration;
using VolPro.Core.Const;
using VolPro.Core.Controllers.Basic;
using VolPro.Core.DBManager;
using VolPro.Core.Extensions;
using VolPro.Core.Infrastructure;
using VolPro.Core.ManageUser;
namespace VolPro.WebApi.Controllers.Builder;

[ApiController]
[Route("api/db/[controller]")]
public class TableController : VolController
{
    private readonly ITableService _tableService;

    public TableController(ITableService tableService)
    {
        _tableService = tableService;
    }
   private static  string[] actions = new string[] { "GetDbService", "GetAllTables", "GetTableInfo", "TableExists" };
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        string actionName= ((ControllerActionDescriptor)context.ActionDescriptor).ActionName;
        if (actions.Contains(actionName))
        {
             base.OnActionExecuting(context);
            return;
        }
        var value = AppSetting.GetSettingString("DbTable").GetInt();
        if (value != 1)
        {
            context.Result = Json(new { status = false, message = "后台[appsettings.json]需要开启创建表属性才可以使用" });
            return;
        }
        if (UserContext.Current.IsSuperAdmin)
        {
             base.OnActionExecuting(context);
            return;
        }
        context.Result = Json(new { status = false, message = "普通帐号没有权限操作" });
        return;
    }
    [HttpPost("getDbService")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public ActionResult GetDbService(string dbService)
    {
        var dic = DictionaryManager.GetDictionary("dbServer");
        var data = dic.Sys_DictionaryList.Select(s =>
          new
          {
              key = s.DicValue,
              value = s.DicName,
              dbType = DbRelativeCache.GetDbType(dic.DBServer)?? DBType.Name
          });
        return Json(data);
    }
    [HttpPost("getAllTables")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<ActionResult<List<string>>> GetAllTables(string dbService)
    {
        var tables = await _tableService.GetAllTablesAsync(dbService);
        return Json(tables);
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpPost, Route("getTableInfo")]
    public async Task<ActionResult<TableInfoDto>> GetTableInfo(string tableName, string dbService)
    {
        var tableInfo = await _tableService.GetTableInfoAsync(dbService, tableName);
        if (tableInfo == null)
        {
            return NotFound($"Table '{tableName}' not found.");
        }
        return Ok(tableInfo);
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpPost("createTable")]
    public async Task<ActionResult> CreateTable([FromBody] CreateTableRequest request, string dbService)
    {
        var res = await _tableService.CreateTableAsync(dbService, request);
        return Json(res);
    }
    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpPost("updateTable")]
    public async Task<ActionResult> UpdateTable([FromBody] UpdateTableRequest request, string tableName, string dbService)
    {
        var res = await _tableService.UpdateTableAsync(dbService, request);
        return Json(res);
    }
    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpPost, Route("deleteTable")]
    public async Task<ActionResult> DeleteTable(string tableName, string dbService)
    {
        var res = await _tableService.DeleteTableAsync(dbService, tableName);
        return Json(res);
    }

    [HttpPost("exists")]
    public async Task<ActionResult<bool>> TableExists(string tableName, string dbService)
    {
        var exists = await _tableService.TableExistsAsync(dbService, tableName);
        return Ok(exists);
    }
}
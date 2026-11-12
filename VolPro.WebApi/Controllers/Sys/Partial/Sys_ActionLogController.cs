/*
 *接口编写处...
*如果接口需要做Action的权限验证，请在Action上使用属性
*如: [ApiActionPermission("Sys_ActionLog",Enums.ActionPermissionOptions.Search)]
 */
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using VolPro.Entity.DomainModels;
using VolPro.Sys.IServices;
using VolPro.Core.UserManager;
using System.Linq;

namespace VolPro.Sys.Controllers
{
    public partial class Sys_ActionLogController
    {
        private readonly ISys_ActionLogService _service;//访问业务代码
        private readonly IHttpContextAccessor _httpContextAccessor;

        [ActivatorUtilitiesConstructor]
        public Sys_ActionLogController(
            ISys_ActionLogService service,
            IHttpContextAccessor httpContextAccessor
        )
        : base(service)
        {
            _service = service;
            _httpContextAccessor = httpContextAccessor;
        }
        [Route("getTableInfo"), HttpPost]
        public IActionResult GetTableInfo(string table)
        {
            var data = TableColumnContext.Data.Where(x => x.TableName == table).Select(s => new { field = s.ColumnName, title = s.ColumnCnName }).ToList();
            var detail = TableColumnContext.TableInfo.Where(x => x.TableTrueName == table).Where(x => !string.IsNullOrEmpty(x.DetailName)).Select(s => new { s.DetailName, s.DetailCnName }).FirstOrDefault();
            if (detail != null)
            {
                var arr = detail.DetailName.Split(",");
                if (arr.Length > 0)
                {
                    var detailColumn = TableColumnContext.Data.Where(x => x.TableName == arr[0]).Select(s => new { field = s.ColumnName, title = s.ColumnCnName, hidden = s.IsDisplay != 1 }).ToList();
                    return JsonNormal(new { data, detail = new { table = arr[0], columns = detailColumn } });
                };
            }
            return JsonNormal(data);
        }
    }
}

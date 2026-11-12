/*
 *代码由框架生成,任何更改都可能导致被代码生成器覆盖
 *如果要增加方法请在当前目录下Partial文件夹Sys_FormWorkFlowAuditLogController编写
 */
using Microsoft.AspNetCore.Mvc;
using VolPro.Core.Controllers.Basic;
using VolPro.Entity.AttributeManager;
using VolPro.Sys.IServices;
namespace VolPro.Sys.Controllers
{
    [Route("api/Sys_FormWorkFlowAuditLog")]
    [PermissionTable(Name = "Sys_FormWorkFlowAuditLog")]
    public partial class Sys_FormWorkFlowAuditLogController : ApiBaseController<ISys_FormWorkFlowAuditLogService>
    {
        public Sys_FormWorkFlowAuditLogController(ISys_FormWorkFlowAuditLogService service)
        : base(service)
        {
        }
    }
}


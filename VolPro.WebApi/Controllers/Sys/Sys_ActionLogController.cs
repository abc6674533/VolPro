/*
 *代码由框架生成,任何更改都可能导致被代码生成器覆盖
 *如果要增加方法请在当前目录下Partial文件夹Sys_ActionLogController编写
 */
using Microsoft.AspNetCore.Mvc;
using VolPro.Core.Controllers.Basic;
using VolPro.Entity.AttributeManager;
using VolPro.Sys.IServices;
namespace VolPro.Sys.Controllers
{
    [Route("api/Sys_ActionLog")]
    [PermissionTable(Name = "Sys_ActionLog")]
    public partial class Sys_ActionLogController : ApiBaseController<ISys_ActionLogService>
    {
        public Sys_ActionLogController(ISys_ActionLogService service)
        : base(service)
        {
        }
    }
}


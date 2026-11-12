/*
 *代码由框架生成,任何更改都可能导致被代码生成器覆盖
 *如果要增加方法请在当前目录下Partial文件夹Sys_FormWorkFlowApplyStepController编写
 */
using Microsoft.AspNetCore.Mvc;
using VolPro.Core.Controllers.Basic;
using VolPro.Entity.AttributeManager;
using VolPro.Sys.IServices;
namespace VolPro.Sys.Controllers
{
    [Route("api/Sys_FormWorkFlowApplyStep")]
    [PermissionTable(Name = "Sys_FormWorkFlowApplyStep")]
    public partial class Sys_FormWorkFlowApplyStepController : ApiBaseController<ISys_FormWorkFlowApplyStepService>
    {
        public Sys_FormWorkFlowApplyStepController(ISys_FormWorkFlowApplyStepService service)
        : base(service)
        {
        }
    }
}


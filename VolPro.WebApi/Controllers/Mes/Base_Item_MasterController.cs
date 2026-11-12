/*
 *代码由框架生成,任何更改都可能导致被代码生成器覆盖
 *如果要增加方法请在当前目录下Partial文件夹Base_Item_MasterController编写
 */
using Microsoft.AspNetCore.Mvc;
using VolPro.Core.Controllers.Basic;
using VolPro.Entity.AttributeManager;
using VolPro.Mes.IServices;
namespace VolPro.Mes.Controllers
{
    [Route("api/Base_Item_Master")]
    [PermissionTable(Name = "Base_Item_Master")]
    public partial class Base_Item_MasterController : ApiBaseController<IBase_Item_MasterService>
    {
        public Base_Item_MasterController(IBase_Item_MasterService service)
        : base(service)
        {
        }
    }
}


/*
 *
 *
 *代码由框架生成,此处任何更改都可能导致被代码生成器覆盖
 *所有业务编写全部应在Partial文件夹下Base_Item_MasterService与IBase_Item_MasterService中编写
 */
using VolPro.Mes.IRepositories;
using VolPro.Mes.IServices;
using VolPro.Core.BaseProvider;
using VolPro.Core.Extensions.AutofacManager;
using VolPro.Entity.DomainModels;

namespace VolPro.Mes.Services
{
    public partial class Base_Item_MasterService : ServiceBase<Base_Item_Master, IBase_Item_MasterRepository>
    , IBase_Item_MasterService, IDependency
    {
    public static IBase_Item_MasterService Instance
    {
      get { return AutofacContainerModule.GetService<IBase_Item_MasterService>(); } }
    }
 }

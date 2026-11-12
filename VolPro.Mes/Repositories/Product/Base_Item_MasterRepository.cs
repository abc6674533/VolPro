/*
 *代码由框架生成,任何更改都可能导致被代码生成器覆盖
 *Repository提供数据库操作，如果要增加数据库操作请在当前目录下Partial文件夹Base_Item_MasterRepository编写代码
 */
using VolPro.Mes.IRepositories;
using VolPro.Core.BaseProvider;
using VolPro.Core.EFDbContext;
using VolPro.Core.Extensions.AutofacManager;
using VolPro.Entity.DomainModels;

namespace VolPro.Mes.Repositories
{
    public partial class Base_Item_MasterRepository : RepositoryBase<Base_Item_Master> , IBase_Item_MasterRepository
    {
    public Base_Item_MasterRepository(ServiceDbContext dbContext)
    : base(dbContext)
    {

    }
    public static IBase_Item_MasterRepository Instance
    {
      get {  return AutofacContainerModule.GetService<IBase_Item_MasterRepository>(); } }
    }
}

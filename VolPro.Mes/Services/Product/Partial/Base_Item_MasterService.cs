/*
 *所有关于Base_Item_Master类的业务代码应在此处编写
*可使用repository.调用常用方法，获取EF/Dapper等信息
*如果需要事务请使用repository.DbContextBeginTransaction
*也可使用DBServerProvider.手动获取数据库相关信息
*用户信息、权限、角色等使用UserContext.Current操作
*Base_Item_MasterService对增、删、改查、导入、导出、审核业务代码扩展参照ServiceFunFilter
*/
using VolPro.Core.BaseProvider;
using VolPro.Core.Extensions;
using VolPro.Core.Extensions.AutofacManager;
using VolPro.Core.Utilities;
using VolPro.Entity.DomainModels;
using VolPro.Mes.IRepositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using StackExchange.Redis;
using System.Linq;
using System.Linq.Expressions;

namespace VolPro.Mes.Services
{
    public partial class Base_Item_MasterService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IBase_Item_MasterRepository _repository;//访问数据库

        [ActivatorUtilitiesConstructor]
        public Base_Item_MasterService(
            IBase_Item_MasterRepository dbRepository,
            IHttpContextAccessor httpContextAccessor
            )
        : base(dbRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _repository = dbRepository;
            //多租户会用到这init代码，其他情况可以不用
            //base.Init(dbRepository);
        }

        public override WebResponseContent Add(SaveModel saveDataModel)
        {
            WebResponseContent webResponse = new WebResponseContent();

            // 在保存数据库前的操作，所有数据都验证通过了，这一步执行完就执行数据库保存
            AddOnExecuting = (Base_Item_Master order, object list) =>
            {
                Base_Item_Master MasterModel = _repository.FindFirst(model => model.ItemCode == order.ItemCode);

                if (MasterModel != null)
                {
                    return webResponse.Error("物料号不能重复！");
                }
                return webResponse.OK();
            };

            return base.Add(saveDataModel);
        }

        public override WebResponseContent Import(List<IFormFile> files)
        {
            WebResponseContent webResponse = new WebResponseContent();
            //导入保存前处理(可以对list设置新的值)
            ImportOnExecuting = (List<Base_Item_Master> list) =>
            {
                bool b = true;
                foreach (Base_Item_Master item in list)
                {
                    Base_Item_Master MasterModel = _repository.FindFirst(model => model.ItemCode == item.ItemCode);
                    //List<Base_Item_Master> list2 = _repository.SqlSugarClient.Queryable(Base_Item_Master => );
                    if (MasterModel != null)
                    {
                        b = false;
                        break;
                    }
                }
                if (!b)
                {
                    return webResponse.Error("物料号不能重复！");
                }
                return webResponse.OK();
            };

            return base.Import(files);
        }





    }
}

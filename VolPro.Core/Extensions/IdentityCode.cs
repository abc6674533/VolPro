using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using VolPro.Core.BaseProvider;
using VolPro.Core.Configuration;
using VolPro.Core.DBManager;
using VolPro.Core.DbSqlSugar;
using VolPro.Core.ManageUser;
using VolPro.Core.Tenancy;
using VolPro.Entity.DomainModels;

namespace VolPro.Core.Extensions
{

    public static class IdentityCode
    {

        private static List<Sys_CodeRule> _codeRules = null;
        private static object ruleObject = new object();

        public static void Init()
        {
            _codeRules = null;
        }

        public static List<Sys_CodeRule> CodeRules
        {
            get
            {
                if (_codeRules == null)
                {
                    lock (ruleObject)
                    {
                        if (_codeRules == null)
                        {
                            _codeRules = DBServerProvider.DbContext.Set<Sys_CodeRule>().ToList();//.FilterTenancy()
                        }
                    }
                }
                return _codeRules;
            }
        }
        /// <summary>
        /// 生成单据号(先在[单据编码]菜单维护规则)
        /// 使用方式：var order = new DemoOrder();  
        ///              order.CreateCode();
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static T CreateCode<T>(this T entity) where T : class, new()
        {
            CreateCodeList(new List<T>() { entity });
            return entity;
        }
        /// <summary>
        /// 批量生成单据号(先在[单据编码]菜单维护规则)
        /// 使用方式：var list =new List<DemoOrder>(){};  
        ///              list.CreateCode();
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static List<T> CreateCodeList<T>(this List<T> entity) where T : class, new()
        {
            var query = CodeRules.Where(x => x.TableName == typeof(T).Name);
            //租户分库
            if (AppSetting.UseDynamicShareDB)
            {
                query = query.Where(x => x.DbServiceId == UserContext.CurrentServiceId);
            }
            else
            {
                if (AppSetting.TenancyField != null)
                {
                    query = query.Where(x => x.TenancyId == UserContext.CurrentServiceId.ToString());
                }
            }
            var rule = query.OrderByDescending(x => x.CreateDate).FirstOrDefault();
            if (rule == null)
            {
                return entity;
            }

            RuleIncremental ruleIncremental = RuleIncremental.day;
            try
            {
                ruleIncremental = (RuleIncremental)Enum.Parse(typeof(RuleIncremental), rule.RuleIncremental);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"单据号枚举转换失败:${rule.RuleIncremental},ex:{ex.Message}");
            }
            CreateList<T>(entity,
                 rule.Field.GetExpression<T>(),
                 rule.PrefixCode,
                 rule.OrderFiled?.GetExpression<T>(),
                 filter: null,
                 startingDay: rule.RuleIncremental == "day",//这里待完,只处理了是否每天每生成与一直接自增
                 dateFormat: rule.RuleType,
                 rule.ValueLen,
                 rule.ConcatenationSymbol,
                 ruleIncremental: ruleIncremental);
            return entity;
        }

        /// <summary>
        /// 创建自增单据号
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">实体对象</param>
        /// <param name="codeField">要设置单据号的字段</param>
        /// <param name="preCode">单据号前缀,如：{TC}{2023}{0001}</param>
        /// <param name="dateFieldExpression">排序字段，每天都从第1个号码开始</param>
        /// <param name="filter">过滤条件</param>
        /// <param name="startingDay">是否每天都从第1个号码开始</param>
        /// <param name="dateFormat">是否生成日期流水号</param>
        /// <param name="len">数字长度</param>
        /// 使用示例：
        ///   Sys_User user=  new Sys_User();
        ///   user.Create(x => x.UserName, "U", x => x.CreateDate);
        /// <returns></returns>
        public static string Create<T>(this T entity,
            Expression<Func<T, object>> codeField,
            string preCode = "Code",
            Expression<Func<T, object>> dateFieldExpression = null,
            Expression<Func<T, bool>> filter = null,
            bool startingDay = true,
            string dateFormat = "yyyyMMdd",
            int len = 4,
            string concatenationSymbol = null,
            RuleIncremental ruleIncremental = RuleIncremental.day
            ) where T : class, new()
        {
            return new List<T>() { entity }.CreateList(codeField, preCode, dateFieldExpression, filter, startingDay, dateFormat, len, concatenationSymbol, ruleIncremental: ruleIncremental);
        }

        public static string CreateList<T>(this List<T> list,
         Expression<Func<T, object>> codeField,
         string preCode = "Code",
         Expression<Func<T, object>> dateFieldExpression = null,
         Expression<Func<T, bool>> filter = null,
         bool startingDay = true,
         string dateFormat = "yyyyMMdd",
         int len = 4,
         string concatenationSymbol = null,
         RuleIncremental ruleIncremental = RuleIncremental.day
         ) where T : class,new()
        {
            if (dateFormat == RuleIncremental.none.ToString())
            {
                dateFormat = null;
            }

            if (concatenationSymbol == null)
            {
                concatenationSymbol = "";
            }
            string dateField;

            if (dateFieldExpression == null)
            {
                dateField = AppSetting.CreateMember.DateField;
            }
            else
            {
                dateField = dateFieldExpression.GetExpressionPropertyFirst();
            }
            var field = codeField.GetExpressionPropertyFirst();


            DateTime? dateNow = null;// (DateTime)DateTime.Now.ToString("yyyy-MM-dd").GetDateTime();
            switch (ruleIncremental)
            {
                case RuleIncremental.none:
                    break;
                case RuleIncremental.day:
                    dateNow = (DateTime)DateTime.Now.ToString("yyyy-MM-dd").GetDateTime();
                    break;
                case RuleIncremental.month:
                    dateNow = (DateTime)DateTime.Now.ToString("yyyy-MM-01").GetDateTime();
                    break;
                case RuleIncremental.year:
                    dateNow = (DateTime)DateTime.Now.ToString("yyyy-01-01").GetDateTime();
                    break;
                default:
                    break;
            }
            Expression<Func<T, bool>> condition = null;
            if (dateNow != null)
            {
                condition = dateField.CreateExpression<T>(dateNow, Enums.LinqExpressionType.ThanOrEqual);
            }

            Expression<Func<T, bool>> conditionStartWdth = null;
            //增加指定开头单据号的查询，避免其他非单据号影响
            if (!string.IsNullOrEmpty(preCode))
            {
                conditionStartWdth = field.CreateExpression<T>(preCode, Enums.LinqExpressionType.LikeStart);
            }
            var select = field.GetExpression<T, string>();
            string rule = dateFormat == null ? preCode : $"{preCode}{concatenationSymbol}{DateTime.Now.ToString(dateFormat)}";
            string dateText = dateNow?.ToString("yyyy-MM-dd") ?? "none";
            //缓存获取自增编号
         
            var property = typeof(T).GetProperty(field);
            string code = null;
            string serviceId = ""; 
            if (AppSetting.UseDynamicShareDB||!string.IsNullOrEmpty(AppSetting.TenancyField))
            {
                serviceId = $"_{UserContext.CurrentServiceId}";
            }
            string key = $"{typeof(T).Name}_{ruleIncremental.ToString()}_{dateText}{serviceId}";
            foreach (var entity in list)
            {
                int number = Increment<T>(key, filter, condition, conditionStartWdth, codeField, startingDay, select, len);
                code = $"{rule}{concatenationSymbol}{(number).ToString("D" + len)}";
                property.SetValue(entity, code);
            }
            return code;
        }

        private static int GetTableLastIncrement<T>(Expression<Func<T, bool>> filter, Expression<Func<T, bool>> condition, Expression<Func<T, bool>> conditionStartWdth, Expression<Func<T, object>> codeField,bool startingDay, Expression<Func<T, string>> select, int len = 4) where T : class,new ()
        {
            string orderNo = DBServerProvider.GetEntityDbContext<T>().Set<T>()
                  // .WhereIF(filter == null && startingDay, condition)
                  .WhereIF(filter != null  , filter)
                  .WhereIF(condition != null, condition)
                  .WhereIF(conditionStartWdth != null, conditionStartWdth)
                  //.FilterTenancy()
                  .OrderByDescending(codeField)
                  .Select(select)
                  .FirstOrDefault()
                       ?.ToString(); 

            int number = 0;
            if (!string.IsNullOrEmpty(orderNo))
            {
                number = orderNo.Substring(orderNo.Length - len).GetInt();
            }
            return number;
        }
        private static readonly ConcurrentDictionary<string, int> _counters = new ConcurrentDictionary<string, int>();

        private static int Increment<T>(string key, Expression<Func<T, bool>> filter, Expression<Func<T, bool>> condition, Expression<Func<T, bool>> conditionStartWdth, Expression<Func<T, object>> codeField, bool startingDay, Expression<Func<T, string>> select, int len = 4) where T : class, new()
        {
            //使用集群部署这里需要修改下，改为读取redis缓存或者取消下面的注释
            //return GetTableLastIncrement(filter, condition, conditionStartWdth, codeField, startingDay, select: select,len: len);

            return _counters.AddOrUpdate( 
                key,
                addValueFactory: (k) => 
                {
                    int num = GetTableLastIncrement(filter, condition, conditionStartWdth, codeField,startingDay,select, len);
                    return num+1;
                },
               (existingKey, existingValue) =>
               {
                   return existingValue + 1;
               }
            );
        }
    }

    public enum RuleIncremental
    {
        none = 0,
        day,
        month,
        year,
    }
}

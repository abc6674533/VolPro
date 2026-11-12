using Azure;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VolPro.Core.DBManager;
using VolPro.Core.DbSqlSugar;
using VolPro.Core.Extensions;
using VolPro.Core.Infrastructure;
using VolPro.Core.ManageUser;
using VolPro.Entity.DomainModels;

namespace VolPro.Core.WorkFlow
{
    public static class WorkFlowGeneric
    {
        /// <summary>
        /// 获取表当前审批流程数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        public static Sys_WorkFlowTable GetTableWorkflow<T>(this T entity, string table = null) where T : class, new()
        {
            table = table ?? typeof(T).GetEntityTableName();
            string key = typeof(T).GetKeyProperty().GetValue(entity).ToString();
            var data = DBServerProvider.DbContext.Set<Sys_WorkFlowTable>().Includes(x => x.Sys_WorkFlowTableStep)
                 .Where(x => x.WorkTable == table && x.WorkTableKey == key)
                 .WhereIF(WorkFlowContainer.CheckTenancy(), x => x.DbServiceId == UserContext.CurrentServiceId)
                 .FirstOrDefault();
            return data;
        }

        /// <summary>
        /// 获取表当前正在审批的节点
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        public static List<Sys_WorkFlowTableStep> GetTableCurrentFlowStep<T>(this T entity, string table=null) where T : class, new()
        {
            var data = entity.GetTableWorkflow(table);
            if (data == null)
            {
                return null;
            }
            var current = data.Sys_WorkFlowTableStep.Where(x => x.StepId == data.CurrentStepId).ToList();
            return current;
        }
        /// <summary>
        /// 获取表当前上一个审批节点
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        public static List<Sys_WorkFlowTableStep> GetTablePreFlowStep<T>(this T entity, string table=null) where T : class, new()
        {
            var data = entity.GetTableWorkflow(table);
            if (data == null)
            {
                return null;
            }
            var current = data.Sys_WorkFlowTableStep.Where(x => x.StepId == data.CurrentStepId).FirstOrDefault();
            if (current == null)
            {
                return null;
            }
            return data.GetPreStep<Sys_WorkFlowTable>(current);
        }

        /// <summary>
        /// 获取表当前下一个审批节点
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        public static List<Sys_WorkFlowTableStep> GetTableNextFlowStep<T>(this T entity, string table = null) where T : class, new()
        {
            var data = entity.GetTableWorkflow(table);
            if (data == null)
            {
                return null;
            }
            var current = data.Sys_WorkFlowTableStep.Where(x => x.StepId == data.CurrentStepId).FirstOrDefault();
            if (current == null)
            {
                return null;
            }
            return data.GetNextStep<Sys_WorkFlowTable>(current);
        }
        /// <summary>
        /// 获取下一个节点
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="workflow"></param>
        /// <param name="current"></param>
        /// <returns></returns>
        public static List<Sys_WorkFlowTableStep> GetNextStep<T>(this Sys_WorkFlowTable workflow, Sys_WorkFlowTableStep current) where T : Sys_WorkFlowTable
        {
            var list = workflow.Sys_WorkFlowTableStep.Where(x => x.StepId == current.NextStepId).ToList();
            //下一个节点抄送
            if (list.Exists(x => x.StepAttrType == StepType.cc.ToString()))
            {
                list = workflow.Sys_WorkFlowTableStep.Where(x => list.Any(c => c.NextStepId == x.StepId)).ToList();
            }
            return list;
        }
        /// <summary>
        /// 获取上一个节点
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="workflow"></param>
        /// <param name="current"></param>
        /// <returns></returns>
        public static List<Sys_WorkFlowTableStep> GetPreStep<T>(this Sys_WorkFlowTable workflow, Sys_WorkFlowTableStep current) where T : Sys_WorkFlowTable
        {
            var list = workflow.Sys_WorkFlowTableStep.Where(x => x.NextStepId == current.StepId).ToList();
            //上一个节点抄送
            if (list.Exists(x => x.StepAttrType == StepType.cc.ToString()))
            {
                list = workflow.Sys_WorkFlowTableStep.Where(x => list.Any(c => c.StepId == x.NextStepId)).ToList();
            }
            return list;
        }
        public static string GetNextStepId<T>(this Sys_WorkFlowTable workflow, Sys_WorkFlowTableStep current) where T : Sys_WorkFlowTable
        {
            return workflow.GetNextStep<Sys_WorkFlowTable>(current).Select(x => x.StepId).FirstOrDefault();
        }
        public static void CreateTitleTemplate<Entity>(Entity entity, Sys_WorkFlowTable flowTable, Sys_WorkFlow workflow)
        {
            if (string.IsNullOrEmpty(workflow.TitleTemplate))
            {
                return;
            }

            var placeholders = new List<string>();
            var matches = Regex.Matches(workflow.TitleTemplate, @"\{#(\w+)\}");
            foreach (Match match in matches)
            {
                string placeholder = match.Groups[0].Value;
                string key = match.Groups[1].Value;

                placeholders.Add(key);
            }
            var tableOptions = DBServerProvider.DbContext.Set<Sys_TableColumn>()
                .Where(c => c.TableName == workflow.WorkTable && placeholders.Contains(c.ColumnName))
                           .Select(s => new
                           {
                               s.ColumnName,
                               s.ColumnCnName,
                               s.DropNo,
                               isDate = s.IsImage == 4,
                               s.ColumnType,
                               s.EditRowNo,
                               s.EditType,
                               s.IsNull
                           }).ToList();

            List<Sys_Dictionary> dictionaries = new List<Sys_Dictionary>();

            var dicNos = tableOptions.Select(s => s.DropNo).ToList();
            if (dicNos.Count > 0)
            {
                dictionaries = DictionaryManager.GetDictionaries(dicNos, true).ToList();
            }

            var properties = typeof(Entity).GetProperties().Where(x => placeholders.Contains(x.Name));

            string titleTemplate = workflow.TitleTemplate;
            foreach (var property in properties)
            {
                string field = property.Name;
                string placeholder = $"{{#{field}}}";
                string value = property.GetValue(entity)?.ToString();

                var option = tableOptions.Where(x => x.ColumnName == field).FirstOrDefault();
                if (option != null && !string.IsNullOrEmpty(value))
                {
                    if (option.isDate)
                    {
                        value = value.GetDateTime().Value.ToString("yyyy-MM-dd");
                    }
                    else if (option.ColumnType == "DateTime")
                    {
                        value = value.GetDateTime().Value.ToString("yyyy-MM-dd HH:mm:sss");
                    }
                    else if (!string.IsNullOrEmpty(option.DropNo))
                    {
                        string val = null;
                        if (option.EditType == "selectList" || option.EditType == "checkbox" || option.EditType == "treeSelect")
                        {
                            string[] arr = value.Split(",");
                            arr = dictionaries.Where(c => c.DicNo == option.DropNo).FirstOrDefault()
                                   ?.Sys_DictionaryList
                                   ?.Where(c => arr.Contains(c.DicValue))?.Select(s => s.DicName)
                                   .ToArray();
                            val = string.Join(",", arr);
                        }
                        else
                        {
                            val = dictionaries.Where(c => c.DicNo == option.DropNo).FirstOrDefault()
                             ?.Sys_DictionaryList
                             ?.Where(c => c.DicValue == value)?.Select(s => s.DicName)
                             .FirstOrDefault();
                        }
                        if (!string.IsNullOrEmpty(val))
                        {
                            value = val;
                        }
                    }
                }
                titleTemplate = titleTemplate.Replace(placeholder, value ?? "");
            }
            flowTable.TitleTemplate = titleTemplate;
        }

        public static List<string> AuditFields = new List<string>() { "auditid", "auditstatus", "auditor", "auditdate", "auditreason" };

        public static List<string> UpdateAuditInfo<T>(T entity, int auditStatus, string auditReason)
        {
            List<string> auditFields = new List<string>();
            var userInfo = UserContext.Current.UserInfo;
            var properties = typeof(T).GetProperties();
            foreach (var item in properties)
            {
                bool isAudit = true;
                switch (item.Name.ToLower())
                {
                    case "auditid":
                        item.SetValue(entity, userInfo.User_Id.ChangeType(item.PropertyType));
                        break;
                    case "auditstatus":
                        item.SetValue(entity, auditStatus);
                        break;
                    case "auditor":
                        item.SetValue(entity, userInfo.UserTrueName);
                        break;
                    case "auditdate":
                        item.SetValue(entity, DateTime.Now);
                        break;
                    case "auditreason":
                        item.SetValue(entity, auditReason);
                        break;
                    default:
                        isAudit = false;
                        break;
                }
                if (isAudit)
                {
                    auditFields.Add(item.Name);
                }
            }
            return auditFields;
        }
        /// <summary>
        /// 判断数据是否在审批中
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="auditProperty"></param>
        /// <returns></returns>
        public static bool CheckAudting<T>(List<T> list, PropertyInfo auditProperty)
        {
            foreach (var entity in list)
            {

                int value = auditProperty.GetValue(entity).GetInt();
                if (value != (int)AuditStatus.审核中 && value != (int)AuditStatus.待审核)
                {
                    return false;
                    // return Response.Error($"只能操作审批中的数据".Translator());
                }
            }
            return true;
        }
    }
}

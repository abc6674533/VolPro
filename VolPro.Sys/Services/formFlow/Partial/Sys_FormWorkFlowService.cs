/*
 *所有关于Sys_FormWorkFlow类的业务代码应在此处编写
*可使用repository.调用常用方法，获取EF/Dapper等信息
*如果需要事务请使用repository.DbContextBeginTransaction
*也可使用DBServerProvider.手动获取数据库相关信息
*用户信息、权限、角色等使用UserContext.Current操作
*Sys_FormWorkFlowService对增、删、改查、导入、导出、审核业务代码扩展参照ServiceFunFilter
*/
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using VolPro.Core.BaseProvider;
using VolPro.Core.Extensions;
using VolPro.Core.Extensions.AutofacManager;
using VolPro.Core.ManageUser;
using VolPro.Core.Utilities;
using VolPro.Core.WorkFlow;
using VolPro.Entity.DomainModels;
using VolPro.Sys.IRepositories;

namespace VolPro.Sys.Services
{
    public partial class Sys_FormWorkFlowService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISys_FormWorkFlowRepository _repository;//访问数据库
        private readonly ISys_FormWorkFlowStepRepository _stepRepository;//访问数据库
        [ActivatorUtilitiesConstructor]
        public Sys_FormWorkFlowService(
            ISys_FormWorkFlowRepository dbRepository,
             ISys_FormWorkFlowStepRepository stepRepository,
            IHttpContextAccessor httpContextAccessor
            )
        : base(dbRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _repository = dbRepository;
            _stepRepository = stepRepository;
        }
        WebResponseContent webResponse = new WebResponseContent();
        public override WebResponseContent Add(SaveModel saveDataModel)
        {
            AddOnExecuting = (Sys_FormWorkFlow workFlow, object list) =>
            {
                workFlow.DbServiceId = UserContext.CurrentServiceId;
                return webResponse.OK();
            };
            return base.Add(saveDataModel);
        }

        List<Sys_FormWorkFlowStep> add;
        List<Sys_FormWorkFlowStep> update;
        public override WebResponseContent Update(SaveModel saveModel)
        {
            if (saveModel.Extra == null)
            {
                return base.Update(saveModel);
            }

            Sys_FormWorkFlow flow = null;
            UpdateOnExecuting = (Sys_FormWorkFlow workFlow, object addList, object updateList, List<object> delKeys) =>
            {
                flow = workFlow;

                //新增的明细
                add = addList as List<Sys_FormWorkFlowStep>;
                var stepsClone = add.Serialize().DeserializeObject<List<Sys_FormWorkFlowStep>>();
                add.Clear();

                var steps = _stepRepository.FindAsIQueryable(x => x.FormWorkFlow_Id == workFlow.FormWorkFlow_Id)
                 .Select(s => new { s.FormWorkStepFlow_Id, s.StepId })
                 .ToList();
                //删除的节点
                var delIds = steps.Where(x => !stepsClone.Any(c => c.StepId == x.StepId))
                 .Select(s => s.FormWorkStepFlow_Id).ToList();
                delKeys.AddRange(delIds.Select(s => s as object));

                //新增的节点
                var newSteps = stepsClone.Where(x => !steps.Any(c => c.StepId == x.StepId))
                .ToList();
                foreach (var item in newSteps)
                {
                    item.FormWorkStepFlow_Id = Guid.NewGuid();
                }
                add.AddRange(newSteps);
                update = updateList as List<Sys_FormWorkFlowStep>;
                //修改的节点
                var updateSteps = stepsClone.Where(x => steps.Any(c => c.StepId == x.StepId))
                .ToList();
                update.AddRange(updateSteps);
                updateSteps.ForEach(x =>
                {
                    x.FormWorkStepFlow_Id = steps.Where(c => c.StepId == x.StepId).Select(s => s.FormWorkStepFlow_Id).FirstOrDefault();
                    foreach (var item in saveModel.DetailData)
                    {
                        if (item["StepId"].ToString() == x.StepId)
                        {
                            item["FormWorkFlow_Id"] = workFlow.FormWorkFlow_Id;
                            item["FormWorkStepFlow_Id"] = x.FormWorkStepFlow_Id;
                        }
                    }
                });
                return webResponse.OK();
            };

            UpdateOnExecuted = (Sys_FormWorkFlow workFlow, object addList, object updateList, List<object> delKeys) =>
            {
                repository.UpdateRange((List<Sys_FormWorkFlowStep>)updateList);
                _stepRepository.DeleteWithKeys(delKeys.ToArray());
                repository.SaveChanges();
                //   WorkFlowManager.UpdateFlowData(workFlow, (List<Sys_WorkFlowStep>)addList);
                return webResponse.OK();
            };


            webResponse = base.Update(saveModel);
            if (webResponse.Status)
            {
                //flow = repository.FindAsIQueryable(x => x.FormWorkFlow_Id == flow.FormWorkFlow_Id).Include(x => x.Sys_FormWorkFlowStep).FirstOrDefault();
                //webResponse = WorkFlowContainer.Instance.AddTable(flow, flow.Sys_FormWorkFlowStep);
            }
            return webResponse;
        }
    }
}

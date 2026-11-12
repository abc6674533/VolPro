/*
 *接口编写处...
*如果接口需要做Action的权限验证，请在Action上使用属性
*如: [ApiActionPermission("Sys_FormWorkFlow",Enums.ActionPermissionOptions.Search)]
 */
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VolPro.Core;
using VolPro.Core.BaseProvider;
using VolPro.Core.Extensions;
using VolPro.Core.Filters;
using VolPro.Core.ManageUser;
using VolPro.Core.Tenancy;
using VolPro.Core.WorkFlow;
using VolPro.Entity.DomainModels;
using VolPro.Sys.IRepositories;
using VolPro.Sys.IServices;

namespace VolPro.Sys.Controllers
{
    public partial class Sys_FormWorkFlowController
    {
        private readonly ISys_FormWorkFlowService _service;//访问业务代码
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISys_FormWorkFlowRepository _formWorkFlowRepository;

        [ActivatorUtilitiesConstructor]
        public Sys_FormWorkFlowController(
            ISys_FormWorkFlowService service,
            ISys_FormWorkFlowRepository formWorkFlowRepository,
            IHttpContextAccessor httpContextAccessor
        )
        : base(service)
        {
            _service = service;
            _formWorkFlowRepository = formWorkFlowRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// 获取工作流程表数据源
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("getFlowList")]
        public async Task<IActionResult> GetFlowList()
        {
            var list = await _formWorkFlowRepository.FindAsIQueryable(x => x.Enable == 1 && x.LineConfig != "" && x.FormOptions != "")
                          .WhereIF(WorkFlowContainer.CheckTenancy(), c => c.DbServiceId == UserContext.CurrentServiceId)
                          .Select(s => new
                          {
                              s.FormWorkFlow_Id,
                              s.Name,
                              s.Code,
                              s.FormType
                          }).ToListAsync();

            return Json(list);
        }
        [ApiActionPermission]
        public override IActionResult Upload(IEnumerable<IFormFile> fileInput)
        {
            return base.Upload(fileInput);
        }
        [HttpGet, Route("getData")]
        public async Task<IActionResult> GetData(Guid id)
        {
            var data = await _formWorkFlowRepository.FindAsIQueryable(x => x.FormWorkFlow_Id == id)
                          .FirstAsync();
            return Json(data);
        }

        [HttpPost, Route("submitWorkflow")]
        public async Task<IActionResult> SubmitWorkflow([FromBody] Sys_FormWorkFlowApplyData flowApplyData, Guid id)
        {
            flowApplyData.WorkFlowApplyData_Id = Guid.NewGuid();
            var list = await _formWorkFlowRepository.FindAsIQueryable(x => x.FormWorkFlow_Id == id)
                .Includes(x => x.Sys_FormWorkFlowStep).ToListAsync();
            var user = UserContext.Current.UserInfo;
            Guid FormWorkFlowApply_Id = Guid.NewGuid();

            var firstNode = list[0].Sys_FormWorkFlowStep.Where(x => x.StepAttrType != StepType.start.ToString()).OrderBy(x => x.OrderId).FirstOrDefault();

            var data = list.Select(s => new Sys_FormWorkFlowApply()
            {
                FormWorkFlowApply_Id = FormWorkFlowApply_Id,
                FormWorkFlow_Id = id,
                TitleTemplate = string.IsNullOrEmpty(s.TitleTemplate) ? s.Name : s.TitleTemplate,
                Name = s.Name,
                Code = s.Code,
                FormType = s.FormType,
                AuditStatus = (int)AuditStatus.审核中,
                Config = s.Config,
                CreateDate = DateTime.Now,
                Creator = user.UserTrueName,
                CreateID = user.User_Id,
                CurrentStepId = firstNode.StepId,
                StepName = firstNode.StepName,
                Sys_FormWorkFlowApplyStep = s.Sys_FormWorkFlowStep.Select(x => new Sys_FormWorkFlowApplyStep()
                {
                    WorkFlowApplyStep_Id = Guid.NewGuid(),
                    FormWorkFlowApply_Id = FormWorkFlowApply_Id,
                    FormWorkFlow_Id = id,
                    StepId = x.StepId,
                    StepName = x.StepName,
                    StepValue = x.StepValue,
                    StepAttrType = x.StepAttrType,
                    OrderId = x.OrderId,
                    CreateDate = DateTime.Now,
                    CreateID = user.User_Id,
                    Creator = user.UserTrueName,
                }).ToList(),
                Sys_FormWorkFlowApplyData = new List<Sys_FormWorkFlowApplyData>() { flowApplyData }
            }).FirstOrDefault();

          await  _formWorkFlowRepository.SqlSugarClient.InsertNav<Sys_FormWorkFlowApply>(data)
                .Include(x => x.Sys_FormWorkFlowApplyStep)
                .Include(x=>x.Sys_FormWorkFlowApplyData).ExecuteCommandAsync();

            //_formWorkFlowRepository.SqlSugarClient.Insertable(data).AddQueue();
            //_formWorkFlowRepository.SqlSugarClient.Insertable(data.Sys_FormWorkFlowApplyStep).AddQueue();
            //await _formWorkFlowRepository.SqlSugarClient
            //    .Insertable(data.Sys_FormWorkFlowApplyData)
            //    .ExecuteCommandAsync();


            return Json(new { status = true, message = "提交成功".Translator() });
        }
    }
}

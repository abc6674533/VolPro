/*
 *代码由框架生成,任何更改都可能导致被代码生成器覆盖
 *如果数据库字段发生变化，请在代码生器重新生成此Model
 */
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlSugar;
using VolPro.Entity.SystemModels;

namespace VolPro.Entity.DomainModels
{
    [Entity(TableCnName = "流程列表",TableName = "Sys_FormWorkFlowApply",DetailTable =  new Type[] { typeof(Sys_FormWorkFlowApplyStep),typeof(Sys_FormWorkFlowApplyData)},DetailTableCnName = "流程明细,流程列表明细数据",DBServer = "SysDbContext")]
    public partial class Sys_FormWorkFlowApply:SysEntity
    {
        /// <summary>
       ///
       /// </summary>
       [SugarColumn(IsPrimaryKey = true)]
       [Key]
       [Display(Name ="FormWorkFlowApply_Id")]
       [MaxLength(36)]
       [Column(TypeName="uniqueidentifier")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public Guid FormWorkFlowApply_Id { get; set; }

       /// <summary>
       ///流程实例id
       /// </summary>
       [Display(Name ="流程实例id")]
       [MaxLength(36)]
       [Column(TypeName="uniqueidentifier")]
       [Editable(true)]
       public Guid? FormWorkFlow_Id { get; set; }

       /// <summary>
       ///标题
       /// </summary>
       [Display(Name ="标题")]
       [Column(TypeName="nvarchar(2000)")]
       [Editable(true)]
       public string TitleTemplate { get; set; }

       /// <summary>
       ///流程名称
       /// </summary>
       [Display(Name ="流程名称")]
       [MaxLength(300)]
       [Column(TypeName="nvarchar(300)")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public string Name { get; set; }

       /// <summary>
       ///流程编号
       /// </summary>
       [Display(Name ="流程编号")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public string Code { get; set; }

       /// <summary>
       ///流程类型
       /// </summary>
       [Display(Name ="流程类型")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       [Editable(true)]
       public string FormType { get; set; }

       /// <summary>
       ///表单配置
       /// </summary>
       [Display(Name ="表单配置")]
       [Column(TypeName="nvarchar(max)")]
       [Editable(true)]
       public string FormOptions { get; set; }

       /// <summary>
       ///配置信息
       /// </summary>
       [Display(Name ="配置信息")]
       [Column(TypeName="nvarchar(max)")]
       [Editable(true)]
       public string Config { get; set; }

       /// <summary>
       ///表单数据
       /// </summary>
       [Display(Name ="表单数据")]
       [Column(TypeName="nvarchar(max)")]
       [Editable(true)]
       public string FormData { get; set; }

       /// <summary>
       ///配置信息
       /// </summary>
       [Display(Name ="配置信息")]
       [Column(TypeName="nvarchar(max)")]
       [Editable(true)]
       public string DetailOptions { get; set; }

       /// <summary>
       ///审批状态
       /// </summary>
       [Display(Name ="审批状态")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? AuditStatus { get; set; }

       /// <summary>
       ///当前节点id
       /// </summary>
       [Display(Name ="当前节点id")]
       [MaxLength(100)]
       [Column(TypeName="nvarchar(100)")]
       [Editable(true)]
       public string CurrentStepId { get; set; }

       /// <summary>
       ///当前节点
       /// </summary>
       [Display(Name ="当前节点")]
       [Column(TypeName="nvarchar(max)")]
       [Editable(true)]
       public string StepName { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="CurrentOrderId")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? CurrentOrderId { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="CreateID")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? CreateID { get; set; }

       /// <summary>
       ///申请人
       /// </summary>
       [Display(Name ="申请人")]
       [MaxLength(30)]
       [Column(TypeName="nvarchar(30)")]
       [Editable(true)]
       public string Creator { get; set; }

       /// <summary>
       ///申请时间
       /// </summary>
       [Display(Name ="申请时间")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? CreateDate { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="Enable")]
       [Column(TypeName="tinyint")]
       [Editable(true)]
       public byte? Enable { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="Modifier")]
       [MaxLength(30)]
       [Column(TypeName="nvarchar(30)")]
       [Editable(true)]
       public string Modifier { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="ModifyDate")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? ModifyDate { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="ModifyID")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? ModifyID { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="DbServiceId")]
       [MaxLength(36)]
       [Column(TypeName="uniqueidentifier")]
       [Editable(true)]
       public Guid? DbServiceId { get; set; }

       [Display(Name ="流程明细")]
       [ForeignKey("FormWorkFlowApply_Id")][Navigate(NavigateType.OneToMany,nameof(FormWorkFlowApply_Id),nameof(FormWorkFlowApply_Id))]
       public List<Sys_FormWorkFlowApplyStep> Sys_FormWorkFlowApplyStep { get; set; }


       [Display(Name ="流程列表明细数据")]
       [ForeignKey("FormWorkFlowApply_Id")][Navigate(NavigateType.OneToMany,nameof(FormWorkFlowApply_Id),nameof(FormWorkFlowApply_Id))]
       public List<Sys_FormWorkFlowApplyData> Sys_FormWorkFlowApplyData { get; set; }


       
    }
}
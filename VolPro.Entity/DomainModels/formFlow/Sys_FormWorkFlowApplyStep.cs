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
    [Entity(TableCnName = "流程列表审批节点",TableName = "Sys_FormWorkFlowApplyStep",DBServer = "SysDbContext")]
    public partial class Sys_FormWorkFlowApplyStep:SysEntity
    {
        /// <summary>
       ///
       /// </summary>
       [SugarColumn(IsPrimaryKey = true)]
       [Key]
       [Display(Name ="WorkFlowApplyStep_Id")]
       [MaxLength(36)]
       [Column(TypeName="uniqueidentifier")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public Guid WorkFlowApplyStep_Id { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="FormWorkFlowApply_Id")]
       [MaxLength(36)]
       [Column(TypeName="uniqueidentifier")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public Guid FormWorkFlowApply_Id { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="FormWorkFlow_Id")]
       [MaxLength(36)]
       [Column(TypeName="uniqueidentifier")]
       [Editable(true)]
       public Guid? FormWorkFlow_Id { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="StepId")]
       [MaxLength(100)]
       [Column(TypeName="nvarchar(100)")]
       [Editable(true)]
       public string StepId { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="StepName")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       [Editable(true)]
       public string StepName { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="StepType")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? StepType { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="StepValue")]
       [Column(TypeName="nvarchar(max)")]
       [Editable(true)]
       public string StepValue { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="OrderId")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? OrderId { get; set; }

       /// <summary>
       ///审核人id
       /// </summary>
       [Display(Name ="审核人id")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? AuditId { get; set; }

       /// <summary>
       ///审核人
       /// </summary>
       [Display(Name ="审核人")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string Auditor { get; set; }

       /// <summary>
       ///审核状态
       /// </summary>
       [Display(Name ="审核状态")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? AuditStatus { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="AuditDate")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? AuditDate { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="Remark")]
       [Column(TypeName="nvarchar(max)")]
       [Editable(true)]
       public string Remark { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="CreateDate")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? CreateDate { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="CreateID")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? CreateID { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="Creator")]
       [MaxLength(30)]
       [Column(TypeName="nvarchar(30)")]
       [Editable(true)]
       public string Creator { get; set; }

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
       [Display(Name ="StepAttrType")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string StepAttrType { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="ParentId")]
       [Column(TypeName="nvarchar(max)")]
       [Editable(true)]
       public string ParentId { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="NextStepId")]
       [MaxLength(100)]
       [Column(TypeName="nvarchar(100)")]
       [Editable(true)]
       public string NextStepId { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="Weight")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? Weight { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="AuditMethod")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? AuditMethod { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="FormOptions")]
       [Column(TypeName="nvarchar(max)")]
       [Editable(true)]
       public string FormOptions { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="SourceType")]
       [MaxLength(100)]
       [Column(TypeName="nvarchar(100)")]
       [Editable(true)]
       public string SourceType { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="AttachFile")]
       [Column(TypeName="nvarchar(2000)")]
       [Editable(true)]
       public string AttachFile { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="AttachType")]
       [Column(TypeName="nvarchar(2000)")]
       [Editable(true)]
       public string AttachType { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="StepEditForm")]
       [Column(TypeName="nvarchar(max)")]
       [Editable(true)]
       public string StepEditForm { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="AttachQty")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? AttachQty { get; set; }

       
    }
}
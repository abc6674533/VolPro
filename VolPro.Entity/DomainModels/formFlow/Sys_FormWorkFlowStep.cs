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
    [Entity(TableCnName = "流程节点",TableName = "Sys_FormWorkFlowStep",DBServer = "SysDbContext")]
    public partial class Sys_FormWorkFlowStep:SysEntity
    {
        /// <summary>
       ///
       /// </summary>
       [SugarColumn(IsPrimaryKey = true)]
       [Key]
       [Display(Name ="FormWorkStepFlow_Id")]
       [MaxLength(36)]
       [Column(TypeName="uniqueidentifier")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public Guid FormWorkStepFlow_Id { get; set; }

       /// <summary>
       ///流程主表id
       /// </summary>
       [Display(Name ="流程主表id")]
       [MaxLength(36)]
       [Column(TypeName="uniqueidentifier")]
       [Editable(true)]
       public Guid? FormWorkFlow_Id { get; set; }

       /// <summary>
       ///流程节点Id
       /// </summary>
       [Display(Name ="流程节点Id")]
       [MaxLength(100)]
       [Column(TypeName="nvarchar(100)")]
       [Editable(true)]
       public string StepId { get; set; }

       /// <summary>
       ///节点名称
       /// </summary>
       [Display(Name ="节点名称")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       [Editable(true)]
       public string StepName { get; set; }

       /// <summary>
       ///节点类型(1=按用户审批,2=按角色审批)
       /// </summary>
       [Display(Name ="节点类型(1=按用户审批,2=按角色审批)")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? StepType { get; set; }

       /// <summary>
       ///审批用户id或角色id
       /// </summary>
       [Display(Name ="审批用户id或角色id")]
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
       ///备注
       /// </summary>
       [Display(Name ="备注")]
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
       [Display(Name ="NextStepIds")]
       [Column(TypeName="nvarchar(max)")]
       [Editable(true)]
       public string NextStepIds { get; set; }

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
       [Display(Name ="AuditRefuse")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? AuditRefuse { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="AuditBack")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? AuditBack { get; set; }

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
       [Display(Name ="SendMail")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? SendMail { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="Filters")]
       [Column(TypeName="nvarchar(max)")]
       [Editable(true)]
       public string Filters { get; set; }

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
       [Display(Name ="Weight")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? Weight { get; set; }

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
       [Display(Name ="AttachType")]
       [Column(TypeName="nvarchar(2000)")]
       [Editable(true)]
       public string AttachType { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="AllowUpload")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? AllowUpload { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="AttachQty")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? AttachQty { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="LinkMenu")]
       [Column(TypeName="nvarchar(2000)")]
       [Editable(true)]
       public string LinkMenu { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="Config")]
       [Column(TypeName="nvarchar(max)")]
       [Editable(true)]
       public string Config { get; set; }

       
    }
}
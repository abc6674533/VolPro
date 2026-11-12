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
    [Entity(TableCnName = "流程管理",TableName = "Sys_FormWorkFlow",DetailTable =  new Type[] { typeof(Sys_FormWorkFlowStep)},DetailTableCnName = "流程节点",DBServer = "SysDbContext")]
    public partial class Sys_FormWorkFlow:SysEntity
    {
        /// <summary>
       ///
       /// </summary>
       [SugarColumn(IsPrimaryKey = true)]
       [Key]
       [Display(Name ="FormWorkFlow_Id")]
       [MaxLength(36)]
       [Column(TypeName="uniqueidentifier")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public Guid FormWorkFlow_Id { get; set; }

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
       ///图标
       /// </summary>
       [Display(Name ="图标")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       [Editable(true)]
       public string Icon { get; set; }

       /// <summary>
       ///父级id
       /// </summary>
       [Display(Name ="父级id")]
       [MaxLength(36)]
       [Column(TypeName="uniqueidentifier")]
       [Editable(true)]
       public Guid? ParentId { get; set; }

       /// <summary>
       ///流程类型
       /// </summary>
       [Display(Name ="流程类型")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       [Editable(true)]
       public string FormType { get; set; }

       /// <summary>
       ///标题模板
       /// </summary>
       [Display(Name ="标题模板")]
       [Column(TypeName="nvarchar(2000)")]
       [Editable(true)]
       public string TitleTemplate { get; set; }

       /// <summary>
       ///节点信息
       /// </summary>
       [Display(Name ="节点信息")]
       [Column(TypeName="nvarchar(max)")]
       [Editable(true)]
       public string NodeConfig { get; set; }

       /// <summary>
       ///连接配置
       /// </summary>
       [Display(Name ="连接配置")]
       [Column(TypeName="nvarchar(max)")]
       [Editable(true)]
       public string LineConfig { get; set; }

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
       ///配置信息
       /// </summary>
       [Display(Name ="配置信息")]
       [Column(TypeName="nvarchar(max)")]
       [Editable(true)]
       public string DetailOptions { get; set; }

       /// <summary>
       ///备注
       /// </summary>
       [Display(Name ="备注")]
       [Column(TypeName="nvarchar(max)")]
       [Editable(true)]
       public string Remark { get; set; }

       /// <summary>
       ///权重
       /// </summary>
       [Display(Name ="权重")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? Weight { get; set; }

       /// <summary>
       ///显示排序
       /// </summary>
       [Display(Name ="显示排序")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? OrderNo { get; set; }

       /// <summary>
       ///是否可用
       /// </summary>
       [Display(Name ="是否可用")]
       [Column(TypeName="tinyint")]
       [Editable(true)]
       public byte? Enable { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="CreateID")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? CreateID { get; set; }

       /// <summary>
       ///创建 人
       /// </summary>
       [Display(Name ="创建 人")]
       [MaxLength(30)]
       [Column(TypeName="nvarchar(30)")]
       [Editable(true)]
       public string Creator { get; set; }

       /// <summary>
       ///创建时间
       /// </summary>
       [Display(Name ="创建时间")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? CreateDate { get; set; }

       /// <summary>
       ///修改人
       /// </summary>
       [Display(Name ="修改人")]
       [MaxLength(30)]
       [Column(TypeName="nvarchar(30)")]
       [Editable(true)]
       public string Modifier { get; set; }

       /// <summary>
       ///修改时间
       /// </summary>
       [Display(Name ="修改时间")]
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
       [Display(Name ="AuditingEdit")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? AuditingEdit { get; set; }

       /// <summary>
       ///
       /// </summary>
       [Display(Name ="DbServiceId")]
       [MaxLength(36)]
       [Column(TypeName="uniqueidentifier")]
       [Editable(true)]
       public Guid? DbServiceId { get; set; }

       /// <summary>
       ///默认审批状态
       /// </summary>
       [Display(Name ="默认审批状态")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? DefaultAuditStatus { get; set; }

       [Display(Name ="流程节点")]
       [ForeignKey("FormWorkFlow_Id")][Navigate(NavigateType.OneToMany,nameof(FormWorkFlow_Id),nameof(FormWorkFlow_Id))]
       public List<Sys_FormWorkFlowStep> Sys_FormWorkFlowStep { get; set; }


       
    }
}
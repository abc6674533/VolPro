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
    [SugarTable("Sys_ActionLog")]
    [Entity(TableCnName = "审计日志", TableName = "Sys_ActionLog", DBServer = "SysDbContext")]
    public partial class Sys_ActionLog : SysEntity
    {
        /// <summary>
        ///日志Id
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        [Key]
        [Display(Name = "日志Id")]
        [Column(TypeName = "uniqueidentifier")]
        [Editable(true)]
        [Required(AllowEmptyStrings = false)]
        public Guid ActionLogId { get; set; }

        /// <summary>
        ///数据库表
        /// </summary>
        [Display(Name = "数据库表")]
        [MaxLength(100)]
        [Column(TypeName = "nvarchar(100)")]
        [Editable(true)]
        public string TableName { get; set; }

        /// <summary>
        ///业务表名
        /// </summary>
        [Display(Name = "业务表名")]
        [MaxLength(128)]
        [Column(TypeName = "nvarchar(128)")]
        [Editable(true)]
        public string TableCNName { get; set; }

        /// <summary>
        ///操作类型
        /// </summary>
        [Display(Name = "操作类型")]
        [MaxLength(10)]
        [Column(TypeName = "nvarchar(10)")]
        [Editable(true)]
        public string OperationType { get; set; }

        /// <summary>
        ///原始数据
        /// </summary>
        [Display(Name = "原始数据")]
        [Column(TypeName = "nvarchar(max)")]
        [Editable(true)]
        public string OriginalData { get; set; }

        /// <summary>
        ///变更数据
        /// </summary>
        [Display(Name = "变更数据")]
        [Column(TypeName = "nvarchar(max)")]
        [Editable(true)]
        public string ChangedData { get; set; }

        /// <summary>
        ///请求参数
        /// </summary>
        [Display(Name = "请求参数")]
        [Column(TypeName = "nvarchar(max)")]
        [Editable(true)]
        public string RequestParameter { get; set; }

        /// <summary>
        ///请求接口
        /// </summary>
        [Display(Name = "请求接口")]
        [MaxLength(1000)]
        [Column(TypeName = "nvarchar(1000)")]
        [Editable(true)]
        public string Url { get; set; }

        /// <summary>
        ///租户id
        /// </summary>
        [Display(Name = "租户id")]
        [Column(TypeName = "uniqueidentifier")]
        [Editable(true)]
        public Guid? DbServiceId { get; set; }

        /// <summary>
        ///租户id
        /// </summary>
        [Display(Name = "租户id")]
        [MaxLength(100)]
        [Column(TypeName = "nvarchar(100)")]
        [Editable(true)]
        public string TenancyId { get; set; }

        /// <summary>
        ///
        /// </summary>
        [Display(Name = "Remark")]
        [MaxLength(500)]
        [Column(TypeName = "nvarchar(500)")]
        [Editable(true)]
        public string Remark { get; set; }

        /// <summary>
        ///
        /// </summary>
        [Display(Name = "Options")]
        [MaxLength(500)]
        [Column(TypeName = "nvarchar(500)")]
        [Editable(true)]
        public string Options { get; set; }

        /// <summary>
        ///
        /// </summary>
        [Display(Name = "CreateID")]
        [Column(TypeName = "int")]
        [Editable(true)]
        public int? CreateID { get; set; }

        /// <summary>
        ///创建人
        /// </summary>
        [Display(Name = "创建人")]
        [MaxLength(100)]
        [Column(TypeName = "nvarchar(100)")]
        [Editable(true)]
        public string Creator { get; set; }

        /// <summary>
        ///创建时间
        /// </summary>
        [Display(Name = "创建时间")]
        [Column(TypeName = "datetime")]
        [Editable(true)]
        public DateTime? CreateDate { get; set; }

        /// <summary>
        ///
        /// </summary>
        [Display(Name = "ModifyID")]
        [Column(TypeName = "int")]
        [Editable(true)]
        public int? ModifyID { get; set; }

        /// <summary>
        ///
        /// </summary>
        [Display(Name = "Modifier")]
        [MaxLength(100)]
        [Column(TypeName = "nvarchar(100)")]
        [Editable(true)]
        public string Modifier { get; set; }

        /// <summary>
        ///
        /// </summary>
        [Display(Name = "ModifyDate")]
        [Column(TypeName = "datetime")]
        [Editable(true)]
        public DateTime? ModifyDate { get; set; }


    }
}
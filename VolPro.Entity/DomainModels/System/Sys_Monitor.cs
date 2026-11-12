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
using VolPro.Entity.SystemModels;

namespace VolPro.Entity.DomainModels
{
    [Entity(TableCnName = "系统监控",TableName = "Sys_Monitor",DBServer = "SysDbContext")]
    public partial class Sys_Monitor:SysEntity
    {
        /// <summary>
       ///主键ID
       /// </summary>
       [Key]
       [Display(Name ="主键ID")]
       [Column(TypeName="bigint")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public long Id { get; set; }

       /// <summary>
       ///主机名
       /// </summary>
       [Display(Name ="主机名")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public string HostName { get; set; }

       /// <summary>
       ///IP地址
       /// </summary>
       [Display(Name ="IP地址")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public string Ip { get; set; }

       /// <summary>
       ///MAC地址
       /// </summary>
       [Display(Name ="MAC地址")]
       [MaxLength(100)]
       [Column(TypeName="nvarchar(100)")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public string MacAddress { get; set; }

       /// <summary>
       ///CPU使用率(%)
       /// </summary>
       [Display(Name ="CPU使用率(%)")]
       [DisplayFormat(DataFormatString="5,2")]
       [Column(TypeName="decimal")]
       [Editable(true)]
       public decimal? CpuUsage { get; set; }

       /// <summary>
       ///内存使用率(%)
       /// </summary>
       [Display(Name ="内存使用率(%)")]
       [DisplayFormat(DataFormatString="5,2")]
       [Column(TypeName="decimal")]
       [Editable(true)]
       public decimal? MemoryUsage { get; set; }

       /// <summary>
       ///带宽使用情况(例：入站100MB/s,出站50MB/s)
       /// </summary>
       [Display(Name ="带宽使用情况(例：入站100MB/s,出站50MB/s)")]
       [MaxLength(200)]
       [Column(TypeName="nvarchar(200)")]
       [Editable(true)]
       public string BandwidthUsage { get; set; }

       /// <summary>
       ///磁盘使用情况(JSON字符串)
       /// </summary>
       [Display(Name ="磁盘使用情况(JSON字符串)")]
       [Column(TypeName="nvarchar(max)")]
       [Editable(true)]
       public string DiskInfo { get; set; }

       /// <summary>
       ///记录时间
       /// </summary>
       [Display(Name ="记录时间")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? RecordTime { get; set; }

       
    }
}
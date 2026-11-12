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
    [Entity(TableCnName = "料品信息",TableName = "Base_Item_Master",DBServer = "ServiceDbContext")]
    public partial class Base_Item_Master:ServiceEntity
    {
        /// <summary>
       ///guid
       /// </summary>
       [SugarColumn(IsPrimaryKey = true)]
       [Key]
       [Display(Name ="guid")]
       [MaxLength(32)]
       [Column(TypeName="nvarchar(32)")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public string GUID { get; set; }

       /// <summary>
       ///料号
       /// </summary>
       [Display(Name ="料号")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public string ItemCode { get; set; }

       /// <summary>
       ///料名
       /// </summary>
       [Display(Name ="料名")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public string ItemName { get; set; }

       /// <summary>
       ///料号版本
       /// </summary>
       [Display(Name ="料号版本")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string Version { get; set; }

       /// <summary>
       ///旧的料号
       /// </summary>
       [Display(Name ="旧的料号")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string Old_ItemCode { get; set; }

       /// <summary>
       ///旧的料名
       /// </summary>
       [Display(Name ="旧的料名")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string Old_ItemName { get; set; }

       /// <summary>
       ///库存分类
       /// </summary>
       [Display(Name ="库存分类")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string StockCategory { get; set; }

       /// <summary>
       ///料品属性
       /// </summary>
       [Display(Name ="料品属性")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public string ItemFeature { get; set; }

       /// <summary>
       ///存储地点代码
       /// </summary>
       [Display(Name ="存储地点代码")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string StorageCode { get; set; }

       /// <summary>
       ///存储地点名
       /// </summary>
       [Display(Name ="存储地点名")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string StorageName { get; set; }

       /// <summary>
       ///是否生效
       /// </summary>
       [Display(Name ="是否生效")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string Effective { get; set; }

       /// <summary>
       ///规格
       /// </summary>
       [Display(Name ="规格")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string Specs { get; set; }

       /// <summary>
       ///单位
       /// </summary>
       [Display(Name ="单位")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public string Unit { get; set; }

       /// <summary>
       ///所属部门
       /// </summary>
       [Display(Name ="所属部门")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string Department { get; set; }

       /// <summary>
       ///所属部门代码
       /// </summary>
       [Display(Name ="所属部门代码")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string DepartmentCode { get; set; }

       /// <summary>
       ///长
       /// </summary>
       [Display(Name ="长")]
       [DisplayFormat(DataFormatString="18,4")]
       [Column(TypeName="decimal")]
       [Editable(true)]
       public decimal? Long { get; set; }

       /// <summary>
       ///宽
       /// </summary>
       [Display(Name ="宽")]
       [DisplayFormat(DataFormatString="18,4")]
       [Column(TypeName="decimal")]
       [Editable(true)]
       public decimal? Wide { get; set; }

       /// <summary>
       ///高
       /// </summary>
       [Display(Name ="高")]
       [DisplayFormat(DataFormatString="18,4")]
       [Column(TypeName="decimal")]
       [Editable(true)]
       public decimal? High { get; set; }

       /// <summary>
       ///对外销售型号
       /// </summary>
       [Display(Name ="对外销售型号")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string KDCode { get; set; }

       /// <summary>
       ///客户料号
       /// </summary>
       [Display(Name ="客户料号")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string CustomerItemCode { get; set; }

       /// <summary>
       ///材质
       /// </summary>
       [Display(Name ="材质")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string Material { get; set; }

       /// <summary>
       ///密度
       /// </summary>
       [Display(Name ="密度")]
       [DisplayFormat(DataFormatString="18,4")]
       [Column(TypeName="decimal")]
       [Editable(true)]
       public decimal? Density { get; set; }

       /// <summary>
       ///最小包装
       /// </summary>
       [Display(Name ="最小包装")]
       [DisplayFormat(DataFormatString="18,4")]
       [Column(TypeName="decimal")]
       [Editable(true)]
       public decimal? MinPack { get; set; }

       /// <summary>
       ///净重
       /// </summary>
       [Display(Name ="净重")]
       [DisplayFormat(DataFormatString="18,4")]
       [Column(TypeName="decimal")]
       [Editable(true)]
       public decimal? NetWeight { get; set; }

       /// <summary>
       ///毛重
       /// </summary>
       [Display(Name ="毛重")]
       [DisplayFormat(DataFormatString="18,4")]
       [Column(TypeName="decimal")]
       [Editable(true)]
       public decimal? GrossWeight { get; set; }

       /// <summary>
       ///扩展字段1
       /// </summary>
       [Display(Name ="扩展字段1")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string ExtendField1 { get; set; }

       /// <summary>
       ///扩展字段2
       /// </summary>
       [Display(Name ="扩展字段2")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string ExtendField2 { get; set; }

       /// <summary>
       ///扩展字段3
       /// </summary>
       [Display(Name ="扩展字段3")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string ExtendField3 { get; set; }

       /// <summary>
       ///扩展字段4
       /// </summary>
       [Display(Name ="扩展字段4")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string ExtendField4 { get; set; }

       /// <summary>
       ///扩展字段5
       /// </summary>
       [Display(Name ="扩展字段5")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string ExtendField5 { get; set; }

       /// <summary>
       ///扩展字段6
       /// </summary>
       [Display(Name ="扩展字段6")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string ExtendField6 { get; set; }

       /// <summary>
       ///扩展字段7
       /// </summary>
       [Display(Name ="扩展字段7")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string ExtendField7 { get; set; }

       /// <summary>
       ///扩展字段8
       /// </summary>
       [Display(Name ="扩展字段8")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string ExtendField8 { get; set; }

       /// <summary>
       ///扩展字段9
       /// </summary>
       [Display(Name ="扩展字段9")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string ExtendField9 { get; set; }

       /// <summary>
       ///扩展字段10
       /// </summary>
       [Display(Name ="扩展字段10")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string ExtendField10 { get; set; }

       /// <summary>
       ///租户ID
       /// </summary>
       [Display(Name ="租户ID")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string TenancyId { get; set; }

       /// <summary>
       ///软删除
       /// </summary>
       [Display(Name ="软删除")]
       [Column(TypeName="int")]
       [Editable(true)]
       [Required(AllowEmptyStrings=false)]
       public int IsDel { get; set; }

       /// <summary>
       ///创建人ID
       /// </summary>
       [Display(Name ="创建人ID")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? CreateID { get; set; }

       /// <summary>
       ///创建人
       /// </summary>
       [Display(Name ="创建人")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
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
       ///修改人ID
       /// </summary>
       [Display(Name ="修改人ID")]
       [Column(TypeName="int")]
       [Editable(true)]
       public int? ModifyID { get; set; }

       /// <summary>
       ///修改人
       /// </summary>
       [Display(Name ="修改人")]
       [MaxLength(50)]
       [Column(TypeName="nvarchar(50)")]
       [Editable(true)]
       public string Modifier { get; set; }

       /// <summary>
       ///修改时间
       /// </summary>
       [Display(Name ="修改时间")]
       [Column(TypeName="datetime")]
       [Editable(true)]
       public DateTime? ModifyDate { get; set; }

       
    }
}
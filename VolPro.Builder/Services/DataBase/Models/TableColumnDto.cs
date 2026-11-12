namespace VolPro.Builder.Services;

public class TableColumnDto
{
    public string ColumnName { get; set; } = string.Empty;
    public string DataType { get; set; } = "nvarchar";
    public int? Length { get; set; }
    // decimal/numeric 的小数位（scale）
    public int? Scale { get; set; }
    public bool IsNullable { get; set; } = true;
    public bool IsPrimaryKey { get; set; } = false;
    public bool IsIdentity { get; set; } = false;
    public string Comment { get; set; }
    public int Order { get; set; }

    public string DefaultValue { get; set; } = string.Empty;
    /// <summary>
    /// 数据库字段（true=来自数据库，false=新增）
    /// </summary>
    public bool IsDbField { get; set; }

    /// <summary>
    /// 原始字段名（用于区分重命名与删除：重命名时 OriginalColumnName 为旧名）
    /// </summary>
    public string OriginalColumnName { get; set; } = string.Empty;
}
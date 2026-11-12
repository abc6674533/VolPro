using System.Collections.Generic;

namespace VolPro.Builder.Services;

public class TableInfoDto
{
    public string TableName { get; set; } = string.Empty;
    public List<TableColumnDto> Columns { get; set; } = new();
}
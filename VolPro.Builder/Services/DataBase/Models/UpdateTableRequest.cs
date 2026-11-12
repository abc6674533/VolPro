using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VolPro.Builder.Services;

public class UpdateTableRequest
{
    [Required]
    public string TableName { get; set; } = string.Empty;
    
    public List<TableColumnDto> Columns { get; set; } = new();
}
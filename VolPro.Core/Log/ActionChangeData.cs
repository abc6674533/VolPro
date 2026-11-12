using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VolPro.Core.Log
{
    public class ActionChangeQueue {
        public ActionChangeType ActionChangeType { get;set;}
     
        public List<ActionChangeData> Datas { get; set;}
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

        public Guid? DbServiceId { get; set; }

        public string RequestParameter { get; set; }
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
    }

    public class ActionChangeData
    {
        public object OriginalData { get; set; }
        public object ChangedData { get; set; }
    
    }
}

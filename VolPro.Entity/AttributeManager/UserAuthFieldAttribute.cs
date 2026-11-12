using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VolPro.Entity.AttributeManager
{
    public class UserAuthFieldAttribute: Attribute
    {
        /// <summary>
        /// 数据权限要过滤的名称
        /// </summary>
        public string Name { get; set; }
    }
}

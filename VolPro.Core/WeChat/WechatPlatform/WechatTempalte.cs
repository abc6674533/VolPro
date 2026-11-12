using Quartz.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VolPro.Core.DBManager;
using VolPro.Core.Extensions;
using VolPro.Core.ManageUser;
using VolPro.Core.Services;
using VolPro.Entity.DomainModels;
using Logger = VolPro.Core.Services.Logger;

namespace VolPro.Core.WeChat
{
    public class WeChatTempalte
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="unionOpenids">用户的公众号id</param>
        /// <returns></returns>
        public static QueueMessage 订单通知(List<string> unionOpenids)
        {
            var data2 = new QueueMessage()
            {
                AccountId = new List<int>() { },
                unionOpenids= unionOpenids,
                QueueTempalte = QueueTempalte.订单通知,
                TempalteId = "模板id",
                Data = new Dictionary<string, object>() {

                         { "模板字段1", new { value = "值"} },
                         { "模板字段2", new { value =DateTime.Now.ToString("yyyy-MM-dd") } },
                    }
            };
            return data2;
        }
        /// <summary>
        /// 其他类库自己接着添加
        /// </summary>
        /// <param name="unionOpenids">用户的公众号id</param>
        /// <returns></returns>
        public static QueueMessage 自定义通知(List<string> unionOpenids)
        {
            var data2 = new QueueMessage()
            {
                AccountId = new List<int>() { },
                unionOpenids = unionOpenids,
                TempalteId = "模板id",
                Data = new Dictionary<string, object>() {

                         { "模板字段1", new { value = "值"} },
                         { "模板字段2", new { value =DateTime.Now.ToString("yyyy-MM-dd") } },
                    }
            };
            return data2;
        }
    }

    public class QueueMessage
    {
        public string TempalteId { get; set; }
        public string Message { get; set; }
        /// <summary>
        /// 发送模板
        /// </summary>
        public QueueTempalte QueueTempalte { get; set; }

        /// <summary>
        /// 发送模板的数据
        /// </summary>
        public Dictionary<string, object> Data { get; set; }

        /// <summary>
        /// 发送指定的用户
        /// </summary>
        public List<int> AccountId { get; set; }

        public string Phone { get; set; }

        /// <summary>
        /// 微信公从号openid
        /// </summary>
        public List<string> unionOpenids { get; set; }
    }

    public enum QueueTempalte
    {
         订单通知,
         自定义通知
    }
}

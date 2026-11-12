using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VolPro.Core.BackgroundServices.mail
{
    /// <summary>
    /// 邮件消息模型
    /// </summary>
    public class MailRequestMessage
    {
        /// <summary>
        /// 收件人地址
        /// </summary>
        public string To { get; set; }

        /// <summary>
        /// 抄送人地址
        /// </summary>
        public string Cc { get; set; }

        /// <summary>
        /// 密送人地址
        /// </summary>
        public string Bcc { get; set; }

        /// <summary>
        /// 邮件主题
        /// </summary>
        public required string Subject { get; set; }

        /// <summary>
        /// 邮件内容
        /// </summary>
        public required string Body { get; set; }

        /// <summary>
        /// 是否为HTML格式
        /// </summary>
        public bool IsHtml { get; set; } = true;

        /// <summary>
        /// 附件路径列表
        /// </summary>
        public List<string> Attachments { get; set; } = new List<string>();

        /// <summary>
        /// 优先级
        /// </summary>
        public MailPriority Priority { get; set; } = MailPriority.Normal;
    }

    /// <summary>
    /// 邮件优先级
    /// </summary>
    public enum MailPriority
    {
        Low = 0,
        Normal = 1,
        High = 2
    }
}
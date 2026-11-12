using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VolPro.Core.BackgroundServices.mail
{
    /// <summary>
    /// 邮件请求模型，用于队列处理
    /// </summary>
    public class MailRequest
    {
        /// <summary>
        /// 请求ID
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// 邮件消息
        /// </summary>
        public required MailRequestMessage MailMessage { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 重试次数
        /// </summary>
        public int RetryCount { get; set; } = 0;

        /// <summary>
        /// 最大重试次数
        /// </summary>
        public int MaxRetryCount { get; set; } = 5;

        /// <summary>
        /// 下次重试时间
        /// </summary>
        public DateTime? NextRetryTime { get; set; }
    }
}
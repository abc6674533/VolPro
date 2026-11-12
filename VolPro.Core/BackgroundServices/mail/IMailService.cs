using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VolPro.Core.BackgroundServices.mail
{
    /// <summary>
    /// 邮件服务接口
    /// </summary>
    public interface IMailService
    {
        /// <summary>
        /// 直接发送邮件（同步发送）
        /// </summary>
        /// <param name="email">接收人邮箱，多个用逗号分隔</param>
        /// <param name="subject">主题</param>
        /// <param name="body">内容</param>
        /// <param name="isHtml">是否HTML</param>
        /// <returns></returns>
        Task SendAsync(string email, string subject, string body, bool isHtml = true);

        /// <summary>
        /// 异步发送邮件（加入队列）
        /// </summary>
        /// <param name="email">接收人邮箱，多个用逗号分隔</param>
        /// <param name="subject">主题</param>
        /// <param name="body">内容</param>
        /// <param name="isHtml">是否HTML</param>
        /// <returns></returns>
        Task SendQueuedAsync(string email, string subject, string body, bool isHtml = true);

        /// <summary>
        /// 发送邮件（加入队列，同步方法）
        /// </summary>
        /// <param name="email">接收人邮箱，多个用逗号分隔</param>
        /// <param name="subject">主题</param>
        /// <param name="body">内容</param>
        /// <param name="isHtml">是否HTML</param>
        void Send(string email, string subject, string body, bool isHtml = true);
    }
}

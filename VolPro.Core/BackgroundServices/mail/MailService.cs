using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Channels;
using System.Threading.Tasks;
using VolPro.Core.Configuration;
using VolPro.Core.Extensions;

namespace VolPro.Core.BackgroundServices.mail
{
    /// <summary>
    /// 邮件服务实现
    /// </summary>
    public class MailService : IMailService
    {
        // 邮件队列Channel
        private static readonly Channel<MailRequest> _mailQueue;

        // 提供静态访问Channel的方法
        public static Channel<MailRequest> MailQueue => _mailQueue;

        // 邮件配置信息
        private static string address { get; set; }
        private static string authPwd { get; set; }
        private static string name { get; set; }
        private static string host { get; set; }
        private static int port;
        private static bool enableSsl { get; set; }

        static MailService()
        {
            // 初始化邮件队列Channel
            _mailQueue = Channel.CreateBounded<MailRequest>(new BoundedChannelOptions(1000)
            {
                FullMode = BoundedChannelFullMode.Wait
            });

            // 读取邮件配置
            var section = AppSetting.GetSection("Mail");
            address = section["Address"];
            authPwd = section["AuthPwd"];
            name = section["Name"];
            host = section["Host"];
            port = section["Port"].GetInt();
            enableSsl = section["EnableSsl"].GetBool();
        }

        public MailService()
        {
        }

        /// <summary>
        /// 实际发送邮件的核心方法
        /// </summary>
        private async Task SendEmailCoreAsync(string emails, string subject, string body, bool isHtml = true)
        {
            // 解析多个接收人
            var recipients = new List<string>();
            if (!string.IsNullOrEmpty(emails))
            {
                foreach (var addr in emails.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    recipients.Add(addr.Trim());
                }
            }

            if (recipients.Count == 0)
            {
                return;
            }

            // 创建邮件消息
            using var message = new MailMessage
            {
                From = new MailAddress(address, name)
            };

            // 添加收件人
            foreach (var recipient in recipients)
            {
                message.To.Add(recipient);
            }

            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = isHtml;

            // 配置SMTP客户端
            using var smtpClient = new SmtpClient
            {
                Host = host,
                Port = port,
                EnableSsl = enableSsl,
                Credentials = new NetworkCredential(address, authPwd),
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false
            };

            // 发送邮件
            await smtpClient.SendMailAsync(message);
        }

        /// <summary>
        /// 异步发送邮件（加入队列）
        /// </summary>
        private async Task SendEmailQueuedAsync(MailRequestMessage mailMessage)
        {
            var request = new MailRequest
            {
                MailMessage = mailMessage,
                CreatedAt = DateTime.UtcNow
            };
            await _mailQueue.Writer.WriteAsync(request);
        }

        public void Send(string email, string subject, string body, bool isHtml = true)
        {
            if (string.IsNullOrEmpty(email))
            {
                return;
            }
            // 支持多个接收人，用逗号分隔
            var mailMessage = new MailRequestMessage
            {
                To = email,
                Subject = subject,
                Body = body,
                IsHtml = isHtml
            };
            _mailQueue.Writer.TryWrite(new MailRequest
            {
                MailMessage = mailMessage,
                CreatedAt = DateTime.UtcNow
            });
        }

        /// <summary>
        /// 直接发送邮件（同步发送，不加入队列）
        /// </summary>
        public async Task SendAsync(string email, string subject, string body, bool isHtml = true)
        {
            await SendEmailCoreAsync(email, subject, body, isHtml);
        }

        /// <summary>
        /// 异步发送邮件（加入队列）
        /// </summary>
        public async Task SendQueuedAsync(string email, string subject, string body, bool isHtml = true)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    return;
                }

                var mailMessage = new MailRequestMessage
                {
                    To = email,
                    Subject = subject,
                    Body = body,
                    IsHtml = isHtml
                };

                await SendEmailQueuedAsync(mailMessage);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}

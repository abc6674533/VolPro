using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace VolPro.Core.BackgroundServices.mail
{
    /// <summary>
    /// 邮件后台服务，使用Channel队列处理邮件发送
    /// </summary>
    public class MailBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public MailBackgroundService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("发送邮件服务已启动");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // 从队列中读取邮件请求
                    var mailRequest = await MailService.MailQueue.Reader.ReadAsync(stoppingToken);
                    if (mailRequest.RetryCount > mailRequest.MaxRetryCount)
                    {
                        break;
                    }
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var mailService = scope.ServiceProvider.GetRequiredService<IMailService>();

                        try
                        {
                            await mailService.SendAsync(mailRequest.MailMessage.To,
                                mailRequest.MailMessage.Subject,
                                mailRequest.MailMessage.Body,
                                mailRequest.MailMessage.IsHtml);
                        }
                        catch (Exception ex)
                        {
                            mailRequest.RetryCount++;
                            Console.WriteLine($"邮件发送异常:{ex.Message + ex.StackTrace}");
                            Core.Services.Logger.AddAsync($"邮件发送异常:{ex.Message + ex.StackTrace}");
                        }
                    }
                }
                catch (ChannelClosedException)
                {
                    // 队列已关闭，退出循环
                    break;
                }
                catch (OperationCanceledException)
                {
                    // 取消令牌触发，退出循环
                    break;
                }
                catch (Exception)
                {
                    // 邮件队列处理出现未知错误，短暂延迟后继续处理
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            // 关闭队列写入器
            MailService.MailQueue.Writer.Complete();

            // 等待所有待处理的邮件完成
            await base.StopAsync(cancellationToken);
        }
    }
}

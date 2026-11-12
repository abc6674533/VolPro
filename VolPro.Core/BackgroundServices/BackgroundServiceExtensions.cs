using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VolPro.Core.BackgroundServices.mail;
using VolPro.Core.PerformanceMonitor;

namespace VolPro.Core.BackgroundServices
{
    public static class BackgroundServiceExtensions
    {
        public static IServiceCollection AddBackgroundServices(this WebApplicationBuilder builder)
        {
            // 注册邮件服务
            builder.Services.AddSingleton<IMailService, MailService>();
            // 注册邮件后台服务
            builder.Services.AddHostedService<MailBackgroundService>();

            return builder.Services;
        }
    }
}

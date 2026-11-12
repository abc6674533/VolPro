using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VolPro.Core.Log;
using VolPro.Core.PerformanceMonitor;

namespace VolPro.Core.PerformanceMonitor
{
    public static class MonitorExtensions
    {
        public static IServiceCollection AddMonitor(this IServiceCollection services, WebApplicationBuilder builder)
        {

            // 注册性能监控广播器
            services.AddSingleton<IPerformanceMonitorBroadcaster, PerformanceMonitorBroadcaster>();
            // 注册性能监控服务
            services.AddScoped<IPerformanceMonitorService, PerformanceMonitorService>();
            // builder.Configuration

            //  Console.WriteLine("mm" + builder.Configuration["montior"]);
            //发布后才会启动服务监听 !builder.Environment.IsDevelopment() ||
            if (builder.Configuration["montior"] == "1")
            {
                services.AddHostedService<MonitorBackgroundService>();
            }
            // services.AddHostedService<MonitorBackgroundService>();
            return services;
        }


    }
}

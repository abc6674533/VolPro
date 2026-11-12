using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VolPro.Core.SignalR;

namespace VolPro.Core.Log
{
    public static class ActionChangeExtensions
    {
         
        /// <summary>
        /// 注册审计日志
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddActionLog(this IServiceCollection services)
        {
            services.AddSingleton<ActionChangeChannel>();
            services.AddScoped<ActionChangeLog>();
            services.AddHostedService<ActionChangeBackgroundService>();
            return services;
        }
    }
}

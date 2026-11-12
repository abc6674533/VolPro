using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VolPro.Core.PerformanceMonitor
{
    /// <summary>
    /// 性能监控SignalR Hub
    /// </summary>
    public class PerformanceMonitorSignalR : Hub
    {
        private readonly ILogger<PerformanceMonitorSignalR> _logger;

        public PerformanceMonitorSignalR(
            ILogger<PerformanceMonitorSignalR> logger)
        {
            _logger = logger;
        }


        /// <summary>
        /// 客户端连接时调用
        /// </summary>
        public override Task OnConnectedAsync()
        {
            //var connectionId = Context.ConnectionId;
            //_logger.LogInformation($"客户端连接: {connectionId}");
            return base.OnConnectedAsync();
        }

        /// <summary>
        /// 客户端断开连接时调用
        /// </summary>
        public override Task OnDisconnectedAsync(Exception exception)
        {
            //var connectionId = Context.ConnectionId;
            //_logger.LogInformation($"客户端断开连接: {connectionId}");
            return base.OnDisconnectedAsync(exception);
        }

    }
}

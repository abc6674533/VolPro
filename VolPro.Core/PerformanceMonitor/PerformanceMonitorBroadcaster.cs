using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using VolPro.Core.Services;

namespace VolPro.Core.PerformanceMonitor
{
    /// <summary>
    /// 性能监控广播器实现
    /// </summary>
    public class PerformanceMonitorBroadcaster : IPerformanceMonitorBroadcaster
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<PerformanceMonitorBroadcaster> _logger;
        private readonly IHubContext<PerformanceMonitorSignalR> _hubContext;

        public PerformanceMonitorBroadcaster(
            IServiceScopeFactory scopeFactory,
            ILogger<PerformanceMonitorBroadcaster> logger,
            IHubContext<PerformanceMonitorSignalR> hubContext)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _hubContext = hubContext;
        }

        /// <summary>
        /// 广播性能监控数据给所有客户端
        /// </summary>
        public async Task BroadcastPerformanceMetricsAsync()
        {
            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var service = scope.ServiceProvider.GetRequiredService<IPerformanceMonitorService>();
                    var metrics = await service.GetCurrentMetricsAsync();

                    // 将object转换为dynamic以便访问属性
                    dynamic data = metrics;

                    // SignalR推送时过滤掉diskInfo，只推送其他实时更新的数据
                    var signalRData = new
                    {
                        hostName = data.hostName,
                        ip = data.ip,
                        macAddress = data.macAddress,
                        cpuUsage = data.cpuUsage,
                        memoryUsagePercentage = data.memoryUsagePercentage,
                        cacheHitRate = data.cacheHitRate,
                        errorRate = data.errorRate,
                        apiResponseTimes = data.apiResponseTimes,
                        requestCount = data.requestCount,
                        errorCount = data.errorCount,
                        lastUpdateTime = data.lastUpdateTime
                        // 注意：不包含diskInfo，磁盘信息只在首次API调用时获取
                    };

                    // 广播数据给所有连接的客户端
                    await _hubContext.Clients.All.SendAsync("PerformanceMonitorCurrent", signalRData);
                }
            }
            catch (System.Exception ex)
            {
                Logger.AddAsync($"广播性能监控数据失败：{ex.Message}");
            }
        }
    }
}

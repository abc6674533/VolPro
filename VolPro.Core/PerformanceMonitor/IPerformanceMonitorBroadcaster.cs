using System.Threading.Tasks;

namespace VolPro.Core.PerformanceMonitor
{
    /// <summary>
    /// 性能监控广播器接口
    /// </summary>
    public interface IPerformanceMonitorBroadcaster
    {
        /// <summary>
        /// 广播性能监控数据给所有客户端
        /// </summary>
        Task BroadcastPerformanceMetricsAsync();
    }
}
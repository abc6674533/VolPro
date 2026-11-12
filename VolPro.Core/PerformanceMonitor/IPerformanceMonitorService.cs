using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VolPro.Entity.DomainModels;

namespace VolPro.Core.PerformanceMonitor
{
    /// <summary>
    /// 性能监控服务接口
    /// </summary>
    public interface IPerformanceMonitorService
    {
        /// <summary>
        /// 获取当前性能指标
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>性能指标数据</returns>
        Task<object> GetCurrentMetricsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取历史性能数据
        /// </summary>
        /// <param name="from">开始时间</param>
        /// <param name="to">结束时间</param>
        /// <returns>历史性能数据</returns>
        Task<object> GetHistoricalMetricsAsync(DateTime from, DateTime to);
    }
}
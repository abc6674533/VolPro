using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using VolPro.Core.EFDbContext;
using VolPro.Entity.DomainModels;

namespace VolPro.Core.PerformanceMonitor
{
    /// <summary>
    /// 性能监控服务实现
    /// </summary>
    public class PerformanceMonitorService : IPerformanceMonitorService
    {
        private readonly SysDbContext _dbContext;
        private readonly ILogger<PerformanceMonitorService> _logger;

        public PerformanceMonitorService(
            SysDbContext dbContext,
            ILogger<PerformanceMonitorService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// 获取当前服务器的MAC地址
        /// </summary>
        private static string GetCurrentServerMacAddress()
        {
            try
            {
                var ni = NetworkInterface.GetAllNetworkInterfaces()
                    .FirstOrDefault(n =>
                        n.OperationalStatus == OperationalStatus.Up
                        && n.NetworkInterfaceType != NetworkInterfaceType.Loopback
                        && n.NetworkInterfaceType != NetworkInterfaceType.Tunnel);

                if (ni != null)
                {
                    var raw = ni.GetPhysicalAddress()?.GetAddressBytes();
                    if (raw != null && raw.Length > 0)
                    {
                        return string.Join("-", raw.Select(b => b.ToString("X2")));
                    }
                }
            }
            catch { /* 忽略获取MAC地址的异常 */ }

            return "-";
        }

        /// <summary>
        /// 获取当前性能指标
        /// </summary>
        public async Task<object> GetCurrentMetricsAsync(CancellationToken cancellationToken = default)
        {
            // 获取当前服务器的MAC地址
            var currentMacAddress = GetCurrentServerMacAddress();

            // 获取最新的监控数据，按MacAddress过滤
            var latestRecord = await _dbContext.Set<Sys_Monitor>()
                .Where(x => x.MacAddress == currentMacAddress)
                .OrderByDescending(x => x.Id)
                .FirstAsync();

            // 解析磁盘信息
            var diskInfo = new List<object>();
            if (latestRecord != null && !string.IsNullOrWhiteSpace(latestRecord.DiskInfo))
            {
                try
                {
                    diskInfo = JsonConvert.DeserializeObject<List<object>>(latestRecord.DiskInfo) ?? new List<object>();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "解析磁盘信息失败");
                }
            }

            // 获取系统信息
            var systemInfo = GetSystemInformation();

            if (latestRecord == null)
            {
                return new
                {
                    // 服务器信息
                    hostName = "Unknown",
                    ip = "127.0.0.1",
                    macAddress = "-",

                    // 新增系统信息
                    systemArchitecture = systemInfo.Architecture,
                    systemVersion = systemInfo.Version,
                    cpuCoreCount = systemInfo.CpuCoreCount,
                    totalMemory = systemInfo.TotalMemory,
                    usedMemory = systemInfo.UsedMemory,

                    // CPU使用率
                    cpuUsage = 0.0,

                    // 内存使用率
                    memoryUsagePercentage = 0.0,

                    // 缓存命中率
                    cacheHitRate = 0.0,

                    // 错误率
                    errorRate = 0.0,

                    // API响应时间（暂时模拟数据）
                    apiResponseTimes = new List<object>(),

                    // 请求计数
                    requestCount = 0,

                    // 错误计数
                    errorCount = 0,

                    // 磁盘信息
                    diskInfo,

                    // 最后更新时间
                    lastUpdateTime = DateTime.Now
                };
            }

            // 构建返回数据
            var metrics = new
            {
                // 服务器信息
                hostName = latestRecord.HostName ?? "Unknown",
                ip = latestRecord.Ip ?? "127.0.0.1",
                macAddress = latestRecord.MacAddress ?? "-",

                // 新增系统信息
                systemArchitecture = systemInfo.Architecture,
                systemVersion = systemInfo.Version,
                cpuCoreCount = systemInfo.CpuCoreCount,
                totalMemory = systemInfo.TotalMemory,
                usedMemory = systemInfo.UsedMemory,

                // CPU使用率
                cpuUsage = latestRecord.CpuUsage ?? 0,

                // 内存使用率
                memoryUsagePercentage = latestRecord.MemoryUsage ?? 0,

                // 缓存命中率（暂时固定值，实际需要从缓存系统获取）
                cacheHitRate = 85.0,

                // 错误率（暂时固定值，实际需要从日志系统获取）
                errorRate = 0.5,

                // 请求计数
                requestCount = 1250,

                // 错误计数
                errorCount = 6,

                // 磁盘信息
                diskInfo,

                // 最后更新时间
                lastUpdateTime = latestRecord.RecordTime
            };

            return metrics;

        }

        /// <summary>
        /// 获取系统信息
        /// </summary>
        private static dynamic GetSystemInformation()
        {
            try
            {
                // 获取内存信息
                var memoryMetrics = ComputerHelper.GetComputerInfo();

                // 构建系统信息对象
                return new
                {
                    // 系统架构
                    Architecture = RuntimeInformation.OSArchitecture.ToString(),
                    // 系统版本
                    Version = RuntimeInformation.OSDescription,
                    // CPU核心数量
                    CpuCoreCount = Environment.ProcessorCount,
                    // 内存总大小
                    TotalMemory = memoryMetrics.TotalRAM,
                    // 内存使用大小
                    UsedMemory = memoryMetrics.UsedRam
                };
            }
            catch (Exception ex)
            {
                // 记录错误并返回默认值
                Console.WriteLine("获取系统信息失败：" + ex.Message);
                return new
                {
                    Architecture = "Unknown",
                    Version = "Unknown",
                    CpuCoreCount = 0,
                    TotalMemory = "0GB",
                    UsedMemory = "0GB"
                };
            }
        }


        /// <summary>
        /// 获取历史性能数据
        /// </summary>
        public async Task<object> GetHistoricalMetricsAsync(DateTime from, DateTime to)
        {

            var currentMacAddress = GetCurrentServerMacAddress();
            var historicalData = await _dbContext.Set<Sys_Monitor>()
                 // .Where(x => x.RecordTime >= from && x.RecordTime <= to)
                 .Where(x => x.MacAddress == currentMacAddress)
                 .OrderByDescending(x => x.Id)
                                .Select(x => new
                                {
                                    x.Id,
                                    recordTime = x.RecordTime,
                                    cpuUsage = x.CpuUsage,
                                    memoryUsage = x.MemoryUsage,
                                    //bandwidthUsage = x.BandwidthUsage,
                                    //diskInfo = x.DiskInfo
                                })
                .Take(2000).ToListAsync();
            return historicalData.OrderBy(x => x.Id);
        }
    }
}
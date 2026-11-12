using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using VolPro.Core.EFDbContext;
using VolPro.Core.Log;
using VolPro.Core.Services;
using VolPro.Core.Utilities;
using VolPro.Entity.DomainModels;

namespace VolPro.Core.PerformanceMonitor
{
    /// <summary>
    /// 服务器监控后台服务，定时采集服务器信息并写入 Sys_Monitor 表
    /// </summary>
    public class MonitorBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<MonitorBackgroundService> _logger;
        private readonly IPerformanceMonitorBroadcaster _broadcaster;
        /// <summary>采集间隔（毫秒），默认 20 秒</summary>
        private const int IntervalMs = 20_000;

        // 缓存的服务器信息
        private static string _cachedHostName;
        private static string _cachedIp;
        private static string _cachedMac;

        public MonitorBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<MonitorBackgroundService> logger,
            IPerformanceMonitorBroadcaster broadcaster = null)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _broadcaster = broadcaster; // 可以为null，如果没有注册广播器

            // 初始化时缓存服务器信息
            CacheServerInfo();
        }
        /// <summary>
        /// 缓存服务器信息（主机名、IP、MAC地址）
        /// </summary>
        private void CacheServerInfo()
        {
            try
            {
                var (hostName, ip, mac) = GetHostAndNetwork();
                _cachedHostName = hostName ?? "Unknown";
                _cachedIp = ip ?? "127.0.0.1";
                _cachedMac = mac ?? "-";

                //_logger.LogInformation($"服务器信息已缓存 - 主机名: {_cachedHostName}, IP: {_cachedIp}, MAC: {_cachedMac}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "缓存服务器信息失败");
                _cachedHostName = "Unknown";
                _cachedIp = "127.0.0.1";
                _cachedMac = "-";
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            string msg = $"服务性能监控启动，采集间隔：{IntervalMs / 1000} 秒";
            Console.Write(msg);
            Logger.AddAsync(msg);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CollectAndSaveAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "采集或保存服务器监控数据失败");
                }

                try
                {
                    await Task.Delay(IntervalMs, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
            Logger.AddAsync("MonitorBackgroundService 已停止");
        }

        private async Task CollectAndSaveAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SysDbContext>();

            // 使用缓存的服务器信息
            var hostName = _cachedHostName;
            var ip = _cachedIp;
            var mac = _cachedMac;

            var metrics = ComputerHelper.GetComputerInfo();
            var disks = ComputerHelper.GetDiskInfos();

            decimal? cpuUsage = null;
            if (!string.IsNullOrWhiteSpace(metrics?.CPURate) && double.TryParse(metrics.CPURate.TrimEnd('%'), out var cpuVal))
                cpuUsage = (decimal)Math.Min(100, Math.Max(0, cpuVal));

            decimal? memoryUsage = null;
            if (!string.IsNullOrWhiteSpace(metrics?.RAMRate) && double.TryParse(metrics.RAMRate.TrimEnd('%'), out var memVal))
                memoryUsage = (decimal)Math.Min(100, Math.Max(0, memVal));

            string diskInfoJson = null;
            if (disks != null && disks.Count > 0)
            {
                var diskList = disks.Select(d => new
                {
                    d.DiskName,
                    d.TypeName,
                    d.TotalSize,
                    d.Used,
                    d.AvailableFreeSpace,
                    d.AvailablePercent
                }).ToList();
                diskInfoJson = JsonConvert.SerializeObject(diskList);
            }
            var record = new Sys_Monitor
            {
                Id = new IdWorker().NextId(),
                HostName = hostName ?? "Unknown",
                Ip = ip ?? "127.0.0.1",
                MacAddress = mac ?? "-",
                CpuUsage = cpuUsage,
                MemoryUsage = memoryUsage,
                BandwidthUsage = null, // 需基于流量统计，此处暂不实现
                DiskInfo = diskInfoJson,
                RecordTime = DateTime.Now
            };
            await db.SqlSugarClient.Insertable(record).ExecuteCommandAsync();
            //db.Set<Sys_Monitor>().add(record);
            //await db.SaveChangesAsync(cancellationToken);

            //// 数据保存成功后，通过广播器推送给所有连接的客户端
            //if (_broadcaster != null)
            //{
            //    try
            //    {
            //        await _broadcaster.BroadcastPerformanceMetricsAsync();
            //    }
            //    catch (Exception ex)
            //    {
            //        _logger.LogError(ex, "广播性能监控数据失败");
            //    }
            //}
        }

        private static (string hostName, string ip, string mac) GetHostAndNetwork()
        {
            var hostName = Environment.MachineName;
            if (string.IsNullOrWhiteSpace(hostName))
            {
                try { hostName = Dns.GetHostName(); } catch { hostName = "Unknown"; }
            }

            string ip = "127.0.0.1";
            string mac = "-";

            try
            {
                var ni = NetworkInterface.GetAllNetworkInterfaces()
                    .FirstOrDefault(n =>
                        n.OperationalStatus == OperationalStatus.Up
                        && n.NetworkInterfaceType != NetworkInterfaceType.Loopback
                        && n.NetworkInterfaceType != NetworkInterfaceType.Tunnel);

                if (ni != null)
                {
                    var addr = ni.GetIPProperties().UnicastAddresses
                        .FirstOrDefault(a => a.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
                    if (addr != null)
                        ip = addr.Address.ToString();

                    var raw = ni.GetPhysicalAddress()?.GetAddressBytes();
                    if (raw != null && raw.Length > 0)
                        mac = string.Join("-", raw.Select(b => b.ToString("X2")));
                }
            }
            catch { /* 忽略网络信息获取异常 */ }

            return (hostName, ip, mac);
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using VolPro.Core.Controllers.Basic;
using VolPro.Core.PerformanceMonitor;

namespace VolPro.WebApi.Controllers.PerformanceMonitor
{
    [Route("api/Monitor")]
    [ApiController]
    public class MonitorController : VolController
    {
        private readonly IPerformanceMonitorService _monitorService;
        private readonly ILogger<MonitorController> _logger;

        public MonitorController(
            IPerformanceMonitorService monitorService,
            ILogger<MonitorController> logger)
        {
            _monitorService = monitorService;
            _logger = logger;
        }

        /// <summary>
        /// 获取当前性能指标
        /// </summary>
        [HttpGet("current")]
        public async Task<IActionResult> GetCurrentMetrics()
        {
            try
            {
                var metrics = await _monitorService.GetCurrentMetricsAsync(HttpContext.RequestAborted);
                return Ok(new { success = true, data = metrics, message = "获取成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取当前性能指标失败");
                return StatusCode(500, new { success = false, message = "获取性能指标失败" });
            }
        }


        /// <summary>
        /// 获取历史性能数据
        /// </summary>
        /// <param name="from">开始时间</param>
        /// <param name="to">结束时间</param>
        [HttpGet("history")]
        public async Task<IActionResult> GetHistoricalData(
            [FromQuery] DateTime from,
            [FromQuery] DateTime to)
        {
            try
            {
                //// 验证时间范围
                //if (from >= to)
                //{
                //    return BadRequest(new { success = false, message = "开始时间必须早于结束时间" });
                //}

                //// 限制查询范围（最多7天）
                //if ((to - from).TotalDays > 7)
                //{
                //    return BadRequest(new { success = false, message = "查询时间范围不能超过7天" });
                //}

                var data = await _monitorService.GetHistoricalMetricsAsync(from, to);
                return Ok(new { success = true, data = data, message = "获取成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取历史数据失败");
                return StatusCode(500, new { success = false, message = "获取历史数据失败" });
            }
        }
    }
}

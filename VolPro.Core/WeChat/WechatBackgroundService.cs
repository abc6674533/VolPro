using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VolPro.Core.WeChat;

namespace VolPro.Core.SignalR
{
    public class WechatBackgroundService : BackgroundService
    {

        private readonly WechatChannel _channel;
        public WechatBackgroundService( WechatChannel channel)
        {
            _channel=channel;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _channel.Run();
        }
    }

}

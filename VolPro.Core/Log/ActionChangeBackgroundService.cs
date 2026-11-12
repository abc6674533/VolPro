using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VolPro.Core.SignalR;

namespace VolPro.Core.Log
{

    public class ActionChangeBackgroundService : BackgroundService
    {
        private readonly ActionChangeChannel _channel;
        public ActionChangeBackgroundService(ActionChangeChannel channel)
        {
            _channel = channel;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _channel.Run();
        }
    }
}

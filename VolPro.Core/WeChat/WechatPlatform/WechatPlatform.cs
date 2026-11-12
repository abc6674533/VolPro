using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using VolPro.Core.Configuration;
using VolPro.Core.Controllers.DynamicController;
using VolPro.Core.DBManager;
using VolPro.Core.EFDbContext;
using VolPro.Core.Extensions;
using VolPro.Core.Services;
using VolPro.Core.SignalR;
using VolPro.Entity.DomainModels;

namespace VolPro.Core.WeChat
{
    public class WechatPlatform
    {
        private WechatChannel _channel;
        public WechatPlatform(WechatChannel channel)
        {
            _channel=channel;
        }
    
        /// <summary>
        /// 推送公众号信息
        /// </summary>
        public void SendMessage(QueueMessage message)
        {
            _channel.Add(message);
        }
    }  
}
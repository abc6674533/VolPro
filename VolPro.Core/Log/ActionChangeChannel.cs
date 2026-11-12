using Microsoft.AspNetCore.SignalR;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using VolPro.Core.DbSqlSugar;
using VolPro.Core.EFDbContext;
using VolPro.Core.Extensions;
using VolPro.Core.ManageUser;
using VolPro.Core.Services;
using VolPro.Core.SignalR;
using VolPro.Core.WeChat;
using VolPro.Entity.DomainModels;
using Logger = VolPro.Core.Services.Logger;

namespace VolPro.Core.Log
{
    public class ActionChangeChannel
    {
        private bool IsStarted = false;
        private readonly Channel<ActionChangeQueue> _actionChannel = Channel.CreateUnbounded<ActionChangeQueue>();
        public async Task Run()
        {
            Console.WriteLine("审计日志服务已启动");
            IsStarted = true;
            try
            {
                while (await _actionChannel.Reader.WaitToReadAsync())
                {
                    if (_actionChannel.Reader.TryRead(out ActionChangeQueue channelData))
                    {
                        var list = channelData.Datas.Select(s => new Sys_ActionLog()
                        {
                            ActionLogId = Guid.NewGuid(),
                            TableName = channelData.TableName,
                            TableCNName = channelData.TableCNName,
                            OriginalData = s.OriginalData.Serialize(),
                            ChangedData = s.ChangedData.Serialize(),
                            DbServiceId = channelData.DbServiceId,
                            TenancyId = channelData.DbServiceId?.ToString(),
                            RequestParameter = channelData.RequestParameter,
                            OperationType = channelData.ActionChangeType.ToString(),
                            CreateID = channelData.CreateID,
                            CreateDate = channelData.CreateDate,
                            Creator = channelData.Creator
                        }).ToList();
                        DbManger.SysDbContext.Insertable<Sys_ActionLog>(list).ExecuteCommand();
                    }
                }
            }
            catch (Exception ex)
            {
                IsStarted = false;
                string messg = $"日志审计队列异常：{ex.Message},{ex?.StackTrace}";
                Logger.AddAsync(messg);
            }
        }
        public void Write<T>(ActionChangeType changeType, T originalData, T changedData,string requestParameter=null) where T : class
        {
            if (!CheckIsStarted()) return;
            _actionChannel.Writer.TryWrite(GetQueueData<T>(changeType, originalData, changedData,requestParameter));
        }
        public async Task WriteAsync<T>(ActionChangeType changeType, T originalData, T changedData, string requestParameter = null) where T : class
        {
            if (!CheckIsStarted()) return;
            await _actionChannel.Writer.WriteAsync(GetQueueData<T>(changeType, originalData, changedData,requestParameter));
        }
        public void WriteRange<T>(ActionChangeType changeType, List<T> originalData, string requestParameter = null) where T : class
        {
            if (!CheckIsStarted()) return;
            var userInfo = UserContext.Current.UserInfo;
            var data = new ActionChangeQueue()
            {
                ActionChangeType = changeType,
                TableName = typeof(T).GetEntityTableName(),
                TableCNName = typeof(T).GetEntityTableCnName(),
                CreateID = userInfo.User_Id,
                CreateDate = DateTime.Now,
                Creator = userInfo.UserTrueName,
                DbServiceId = UserContext.CurrentServiceId,
                RequestParameter=requestParameter,
                Datas = originalData.Select(s => new ActionChangeData()
                {
                    OriginalData = s
                }).ToList()
            };
            _actionChannel.Writer.TryWrite(data);
        }
        private ActionChangeQueue GetQueueData<T>(ActionChangeType changeType, object originalData, object changedData = null, string requestParameter = null) where T : class
        {
            var userInfo = UserContext.Current.UserInfo;
            return new ActionChangeQueue()
            {
                ActionChangeType = changeType,
                TableName = typeof(T).GetEntityTableName(),
                TableCNName = typeof(T).GetEntityTableCnName(),
                CreateID = userInfo.User_Id,
                CreateDate = DateTime.Now,
                Creator = userInfo.UserTrueName,
                RequestParameter = requestParameter,
                DbServiceId = UserContext.CurrentServiceId,
                Datas = new List<ActionChangeData>()
                {
                    new(){
                        OriginalData= originalData,
                        ChangedData= changedData
                    }
                }
            };
        }
        private bool CheckIsStarted()
        {
            if (!IsStarted)
            {
                Console.WriteLine("审计日志服务未启动，请按更新文档在Program.cs或Startup.cs中开启");
                return false;
            }
            return true;
        }
    }
}

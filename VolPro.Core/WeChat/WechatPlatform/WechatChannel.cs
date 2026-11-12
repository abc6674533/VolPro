using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Channels;
using System.Threading.Tasks;
using VolPro.Core.Configuration;
using VolPro.Core.Extensions;
using VolPro.Core.Services;
using VolPro.Core.WeChat;

namespace VolPro.Core.SignalR
{
    public class WechatChannel
    {
        private Dictionary<string, Access_token> tokenCache = new Dictionary<string, Access_token>();
        private readonly Channel<QueueMessage> _channel = Channel.CreateUnbounded<QueueMessage>();
        private IHttpClientFactory _httpClientFactory;
        private static string platform_appid = "";
        static string platform_secret = "";
        public WechatChannel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            platform_appid = AppSetting.GetSection("wechat")["platform_appid"];
            platform_secret = AppSetting.GetSection("wechat")["platform_secret"];
        }

        public void Add(QueueMessage message)
        {
            _channel.Writer.TryWrite(message);
        }
        public async Task AddAsync(QueueMessage message)
        {
            await _channel.Writer.WriteAsync(message);
        }
        public async Task Run()
        {
            Console.WriteLine("微信公众号消息推送服务已启动");
            await foreach (var data in _channel.Reader.ReadAllAsync())
            {
                Console.WriteLine($"收到消息：{data}");
                try
                {
                    var token = await GetToken();
                    //获取公众号openid
                    //if (data.UnionOpenids == null || data.UnionOpenids.Count == 0)
                    //{
                    //     data.UnionOpenids = await GetUnionOpenid(data.AccountId, data.Phone);
                    //}
                    await SendMessage(token, data, data.unionOpenids);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message + ex.StackTrace);
                    Logger.AddAsync($"微信消推送异常:" + ex.Message + ex.StackTrace);
                }
            }
        }
        /// <summary>
        /// 获取AccessToken
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetToken()
        {
            //token缓存
            Access_token result = null;
            //判断缓存是否存在键：appid，就将缓存中的token赋给result
            //这里要用公众号的id
            if (tokenCache.ContainsKey(platform_appid))
            {
                result = tokenCache[platform_appid];
            }
            //不存在则获取token
            if (result == null || result.CreateTime.AddSeconds(1000) < DateTime.Now)
            {
                string grant_type = "client_credential";
                string tokenUrl = string.Format("https://api.weixin.qq.com/cgi-bin/token?grant_type={0}&appid={1}&secret={2}", grant_type, platform_appid, platform_secret);
                Access_token tokeninfo = (await _httpClientFactory.GetAsync(tokenUrl)).DeserializeObject<Access_token>();
                tokeninfo.CreateTime = DateTime.Now;
                tokenCache[platform_appid] = tokeninfo;
                result = tokeninfo;
            }
            var token = result?.access_token;
            return token;
        }

        private async Task<bool> SendMessage(string strtoken, QueueMessage data, List<string> oplist) //string strtoken)
        {
            //            快递单号
            //{ { character_string5.DATA} }
            //            快递状态
            //{ { phrase6.DATA} }
            //            取送点
            //{ { thing20.DATA} }
            //            预计到仓时间
            //{ { time16.DATA} }
            //根据access_token构建推送接口
            string sendUrl = $@"https://api.weixin.qq.com/cgi-bin/message/template/send?access_token={strtoken}";

            //创建list 循环这里
            //必填 接收者openid
            foreach (var item in oplist)
            {
                if (string.IsNullOrEmpty(item))
                {
                    continue;
                }
                MessageTemplateSendDto mts = new MessageTemplateSendDto();
                mts.template_id = data.TempalteId;
                //switch (data.QueueTempalte)
                //{
                //    case QueueTempalte.预约:
                //        mts.template_id = "iYhw1aKuJuRn0uI0HYshNOlt00yey5DESHlY2Go3yiQ";
                //        break;
                //    default:
                //        break;
                //}
                mts.touser = item;
                //构建请求数据对象
                //必填 模板数据,需要根据模板里面的内容来

                //mts.data = new Dictionary<string, object>() {
                //    {  "thing19", new { value = "保养、洗车" }},
                //    {  "car_number9", new { value = "京A88888" }},
                //    {  "time4", new { value =DateTime.Now.ToString("yyyy-MM-dd")}},
                //    {  "thing24", new { value = "西安汽美一店" } }
                //};
                mts.data = data.Data;
                try
                {
                    var result = await _httpClientFactory.PostAsync(sendUrl, mts);
                    string resMsg = "模板" + mts.Serialize() + "，结果：" + result;
                    Console.WriteLine(resMsg);
                    Logger.AddAsync("消息推送", resMsg);
                }
                catch (Exception e)
                {
                    string resErrorMsg = "模板" + mts.Serialize() + "，结果：" + e.Message + e.StackTrace;
                    Console.WriteLine(resErrorMsg);
                    Logger.AddAsync("消息推送失败", resErrorMsg);
                }
            }
            return true;
        }


        /// <summary>
        /// 根据用户微信公众号openid获取unionid
        /// </summary>
        /// <param name="code"></param>
        /// <param name="token"></param>
        /// <param name="openId"></param>
        /// <param name="unionid"></param>
        /// <returns></returns>
        //public async Task<UserOpenInfo> GetUnionid(string gzhOpenId, string token = null)
        //{

        //    if (string.IsNullOrEmpty(token))
        //    {
        //        token = await GetToken();
        //    }

        //    // 使用授权码换取access token
        //    string accessTokenUrl = $"https://api.weixin.qq.com/cgi-bin/user/info?access_token={token}&openid={gzhOpenId}&lang=zh_CN";

        //    var content = await _httpClientFactory.GetAsync(accessTokenUrl);

        //    UserOpenInfo openInfo = content.DeserializeObject<UserOpenInfo>();
        //    if (string.IsNullOrEmpty(openInfo.Openid))
        //    {
        //        Logger.Error($"获取openid失败,{content}");
        //    }
        //    else
        //    {
        //        Logger.Error($"获取openid成功,{content}");
        //    }

        //    return openInfo;
        //}

        /// <summary>
        /// 根据用户id获取微信公众号openid(不是小程序的openid)
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        //private async Task<List<string>> GetUnionOpenid(List<int> userIds, string phone)
        //{
        //    var dbContext = new SysDbContext() { };

        //    //返回用户的公众号id
        //    //var UnionOpenids=await dbContext.Set<Sys_User>().Where(x => userIds.Contains(x.User_Id)).Select(s => s.UnionOpenid).ToListAsync();
        //    // return UnionOpenids;
        //    await Task.CompletedTask;
        //    return new List<string>();
        //}

    }
    public class MessageTemplateSendDto
    {

        /// <summary>
        /// 必填
        /// 接收者openid
        /// </summary>
        public string touser { get; set; }

        /// <summary>
        /// 必填
        /// 模板ID
        /// </summary>
        public string template_id { get; set; }

        /// <summary>
        /// 必填
        /// 模板数据
        /// </summary>
        public object data { get; set; }

        /// <summary>
        /// 模板内容字体颜色，不填默认为黑色
        /// </summary>
        public string color { get; set; }

        /// <summary>
        /// 防重入id。对于同一个openid + client_msg_id, 只发送一条消息,10分钟有效,超过10分钟不保证效果。若无防重入需求，可不填
        /// </summary>
        public string client_msg_id { get; set; }

    }



    public class MessageTemplateSendDataDto
    {
        public MessageTemplateSendDataContentDto first { get; set; }
        public MessageTemplateSendDataContentDto keyword1 { get; set; }
        public MessageTemplateSendDataContentDto keyword2 { get; set; }
        public MessageTemplateSendDataContentDto keyword3 { get; set; }
        //public MessageTemplateSendDataContentDto keyword4 { get; set; }
        //public MessageTemplateSendDataContentDto remark { get; set; }
    }
    public class MessageTemplateSendDataContentDto
    {
        /// <summary>
        /// 文本内容
        /// </summary>
        public string value { get; set; }

        /// <summary>
        /// 文本颜色
        /// </summary>
        public string color { get; set; }
    }
    public class WxOpenidCodeState
    {

        //返回code
        public string errcode { get; set; }

        // 返回消息
        public string errmsg { get; set; }
    }
    public class PostTage
    {
        //最后一个用户的openid 
        public int tagid { get; set; }
        //第一个拉取的OPENID，不填默认从头开始拉取 
        public string next_openid { get; set; }
    }

    public class Data
    {
        /// <summary>
        /// 
        /// </summary>
        public List<string> openid { get; set; }
    }

    public class TagUserList
    {
        /// <summary>
        /// 
        /// </summary>
        public int count { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Data data { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string next_openid { get; set; }
    }
    /// <summary>
    /// Access_token 的摘要说明
    /// </summary>
    public class Access_token
    {
        public string access_token { get; set; }
        public int expires_in { get; set; } //凭证有效时间
        public string errcode { get; set; } //返回码
        public string errmsg { get; set; } //返回说明
        public DateTime CreateTime { get; set; }
    }

    public class Tags
    {
        /// <summary>
        /// 
        /// </summary>
        public int id { get; set; }
        /// <summary>
        /// 星标组
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int count { get; set; }
    }

    public class Root
    {
        /// <summary>
        /// 
        /// </summary>
        public List<Tags> tags { get; set; }
    }
}

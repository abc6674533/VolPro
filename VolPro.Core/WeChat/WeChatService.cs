using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Web;
using VolPro.Core.Configuration;
using VolPro.Core.EFDbContext;
using VolPro.Core.Enums;
using VolPro.Core.Extensions;
using VolPro.Core.Services;
using VolPro.Core.Utilities;
using VolPro.Entity.DomainModels;

namespace VolPro.Core.WeChat
{
    public class WechatService
    {
        private IHttpClientFactory _httpClientFactory;
        private string AppId { get; set; }
        private string Secret { get; set; }
        public WechatService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            AppId = AppSetting.GetSection("wechat")["appid"];
            Secret = AppSetting.GetSection("wechat")["secret"];
        }

        public async Task<UserOpenInfo> LoginWX(WechatLoginInfo wxInfo, bool getPhone = false)
        {
            WebResponseContent webResponse = new WebResponseContent();
            //获取Access_Token用于解析手机号
            string url = $"https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid={AppId}&secret={Secret}";
            var acccessData = await _httpClientFactory.GetAsync<SesstionResult>(url);
            string access_token = acccessData.data.Access_Token;
            string phone = null;
            if (getPhone)
            {
                ////获取手机号
                url = $"https://api.weixin.qq.com/wxa/business/getuserphonenumber?access_token={access_token}";
                var dic = new Dictionary<string, string> { { "code", wxInfo.PhoneDetail.Code } };
                var result = await _httpClientFactory.PostAsync<PhoneWxInfo>(url, dic, contentType: "application/json");
                phone = result.data.Phone_Info?.PhoneNumber;
                if (phone == null)
                {
                    Logger.Error($"获取手机号失败:code:{wxInfo.Serialize()},返回结果：{result.Serialize()}");
                    //  return webResponse.Error("授权失败");
                }
            }
            UserOpenInfo openInfo = await GetOpenId(wxInfo.Code);
            openInfo.Phone = phone;
            return openInfo;
        }

        public async Task<UserOpenInfo> GetOpenId(string code, string token = null, string openId = null, bool unionid = false)
        {
            //unionid ? $"https://api.weixin.qq.com/cgi-bin/user/info?access_token={token}&openid={openId}&lang=zh_CN"
            // 使用授权码换取access token
            string accessTokenUrl = $"https://api.weixin.qq.com/sns/jscode2session?appid={AppId}&secret={Secret}&js_code={code}&grant_type=authorization_code";
            var res = await _httpClientFactory.GetAsync<UserOpenInfo>(accessTokenUrl);
            if (!res.status)
            {
                Logger.Error($"获取openid失败,{res.Serialize()}");
            }
            return res.data;
        }
    }
}
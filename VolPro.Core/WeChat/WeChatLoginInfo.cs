using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VolPro.Core.WeChat
{
    public class WechatLoginInfo
    {
        /// <summary>
        /// 代码
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// OpenId
        /// </summary>
        public string OpenId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string IV { get; set; }
        /// <summary>
        /// 加密数据
        /// </summary>
        public string EncryptedData { get; set; }
        /// <summary>
        /// 手机号信息
        /// </summary>

        public PhoneDetail PhoneDetail { get; set; }
        //  public string SessionKey { get; set; }
    }

    public class PhoneDetail
    {
        public string Code { get; set; }
        public string IV { get; set; }
        public string EncryptedData { get; set; }
    }

    public class PhoneWxInfo
    {
        public string Errmsg { get; set; }
        public Phone_Info Phone_Info { get; set; }
    }

    /// <summary>
    /// 电话号码
    /// </summary>
    public class Phone_Info
    {
        /// <summary>
        /// 电话号码
        /// </summary>
        public string PhoneNumber { get; set; }
        /// <summary>
        /// 纯电话号码
        /// </summary>
        public string PurePhoneNumber { get; set; }
        /// <summary>
        /// 国家代码
        /// </summary>
        public string CountryCode { get; set; }
    }


    /// <summary>
    /// 微信用户信息
    /// </summary>
    public class WXUserInfo
    {
        /// <summary>
        /// 昵称
        /// </summary>
        public string NickName { get; set; }
        /// <summary>
        /// 性别
        /// </summary>
        public string Gender { get; set; }
        /// <summary>
        /// 省份
        /// </summary>
        public string Province { get; set; }
        /// <summary>
        /// 城市
        /// </summary>
        public string City { get; set; }
        /// <summary>
        /// 国家
        /// </summary>
        public string Country { get; set; }
        /// <summary>
        /// 电话号码
        /// </summary>
        public string PhoneNumber { get; set; }
        /// <summary>
        /// 纯电话号码
        /// </summary>
        public string PurePhoneNumber { get; set; }
        /// <summary>
        /// 国家
        /// </summary>
        public string CountryCode { get; set; }
        /// <summary>
        /// 头像
        /// </summary>
        public string AvatarUrl { get; set; }
        public Watermark Watermark;
    }

    public class Watermark
    {
        public long Timestamp { get; set; }
        public string Appid { get; set; }
    }
    public class SesstionResult
    {
        public string Session_Key { get; set; }
        public string Openid { get; set; }
        public string Token { get; set; }
        public bool IsAuth { get; set; }

        public string Access_Token { get; set; }

    }

}

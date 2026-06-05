using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace YS.Knife.Wechat.MiniProgram
{
    public record class Code2SessionResponse : WechatResponse
    {
        [JsonPropertyName("openid")]
        public string OpenId { get; set; }

        [JsonPropertyName("session_key")]
        public string SessionKey { get; set; }
        [JsonPropertyName("unionid")]
        public string UnionId { get; set; }
    }

    public record class GetAccessTokenResponse : WechatResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
    }
    public record class WechatResponse
    {
        [JsonPropertyName("errcode")]
        public int ErrorCode { get; set; }
        [JsonPropertyName("errmsg")]
        public string ErrorMessage { get; set; }

        public bool IsSuccess => ErrorCode == 0;

    }
    public record GenerateUrlLinkRequest
    {
        [JsonPropertyName("path")]
        public string Path { get; set; }
        [JsonPropertyName("query")]
        public string Query { get; set; }
        [JsonPropertyName("is_expire")]
        public bool? IsExpire { get; set; }
        [JsonPropertyName("expire_type")]
        public int? ExpireType { get; set; }
        [JsonPropertyName("expire_interval")]
        public int? ExpireInterval { get; set; }
        [JsonPropertyName("env_version")]
        public string EnvVersion { get; set; }
    }
    public record GenerateUrlLinkResponse : WechatResponse
    {
        [JsonPropertyName("url_link")]
        public string UrlLink { get; set; }
    }

    public class GetWxacodeunlimitRequest
    {
        /// <summary>场景值，必填，最长32个字符，扫码后通过 options.scene 获取</summary>
        [JsonPropertyName("scene")]
        public string Scene { get; set; }

        /// <summary>页面路径，可选，如 pages/index/index，路径前不要加 "/"</summary>
        [JsonPropertyName("page")]
        public string Page { get; set; }

        /// <summary>二维码宽度，范围280-1280，默认430</summary>
        [JsonPropertyName("width")]
        public int Width { get; set; } = 430;

        /// <summary>自动配置线条颜色，默认false</summary>
        [JsonPropertyName("auto_color")]
        public bool AutoColor { get; set; } = false;

        /// <summary>线条颜色，AutoColor为false时生效</summary>
        [JsonPropertyName("line_color")]
        public LineColor LineColor { get; set; } = null;

        /// <summary>是否透明底色，默认false</summary>
        [JsonPropertyName("is_hyaline")]
        public bool IsHyaline { get; set; } = false;

        /// <summary>要打开的小程序版本，release（线上）或 develop（开发版）</summary>
        [JsonPropertyName("env_version")]
        public string EnvVersion { get; set; } = "release";

        /// <summary>是否校验page路径，true则page必须在现网版本存在</summary>
        [JsonPropertyName("check_path")]
        public bool CheckPath { get; set; } = true;
    }

    public class LineColor
    {
        [JsonPropertyName("r")]
        public int R { get; set; } = 0;

        [JsonPropertyName("g")]
        public int G { get; set; } = 0;

        [JsonPropertyName("b")]
        public int B { get; set; } = 0;
    }
    /// <summary>
    /// 微信消息推送参数
    /// </summary>
    public class WechatNotificationMessage
    {
        /// <summary>
        /// 用户Openid
        /// </summary>
        [JsonPropertyName("touser")]
        public string Touser { get; set; }

        /// <summary>
        /// 模板Id
        /// </summary>
        [JsonPropertyName("template_id")]
        public string TemplateId { get; set; }

        /// <summary>
        /// 跳转页面
        /// </summary>
        [JsonPropertyName("page")]
        public string Page { get; set; }

        /// <summary>
        /// developer为开发版；trial为体验版；formal为正式版；默认为正式版
        /// </summary>
        [JsonPropertyName("miniprogram_state")]
        public string MiniprogramState { get; set; }
    }

    public class WechatNotificationMessage<T> : WechatNotificationMessage
    {
        [JsonPropertyName("data")]
        public T Data { get; set; }
    }
}

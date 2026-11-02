using System.ComponentModel.DataAnnotations;

namespace YS.Knife.Auth
{
    /// <summary>
    /// 第三方登录渠道信息，前端据此渲染登录按钮并跳转授权。
    /// </summary>
    public class ThirdPartyLoginInfo
    {
        /// <summary>
        /// 渠道标识，如 "github"、"wechat"。
        /// </summary>
        [Required]
        public string Provider { get; set; } = null!;

        /// <summary>
        /// 授权跳转地址。
        /// </summary>
        [Required]
        public string AuthorizeUrl { get; set; } = null!;

        /// <summary>
        /// 展示名称，为空时前端可回退显示 <see cref="Provider"/>。
        /// </summary>
        public string? DisplayName { get; set; }
    }
}

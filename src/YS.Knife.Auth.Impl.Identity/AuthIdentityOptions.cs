using System.ComponentModel.DataAnnotations;

namespace YS.Knife.Auth
{
    /// <summary>
    /// Identity 认证配置。仅当使用 Token（JWT）登录时才需要配置 <see cref="SecretKey"/>。
    /// </summary>
    [Options]
    public class AuthIdentityOptions
    {
        /// <summary>
        /// JWT 签名密钥，至少 32 个字符（UTF-8 编码后不少于 32 字节）。
        /// </summary>
        public string? SecretKey { get; set; }

        /// <summary>
        /// JWT 颁发者。
        /// </summary>
        [Required]
        public string Issuer { get; set; } = "YS.Knife";

        /// <summary>
        /// JWT 受众。
        /// </summary>
        [Required]
        public string Audience { get; set; } = "YS.Knife";

        /// <summary>
        /// 访问令牌有效期（分钟）。
        /// </summary>
        [Range(1, 14400)]
        public int AccessTokenExpiresInMinutes { get; set; } = 120;

        /// <summary>
        /// 刷新令牌有效期（天）。
        /// </summary>
        [Range(1, 3650)]
        public int RefreshTokenExpiresInDays { get; set; } = 30;

        /// <summary>
        /// 图形验证码地址。为空表示登录不需要图形验证码。
        /// </summary>
        public string? CaptchaUrl { get; set; }

        /// <summary>
        /// 可用的第三方登录渠道，供登录页渲染。
        /// </summary>
        public ThirdPartyLoginInfo[] ThirdPartyLogins { get; set; } = Array.Empty<ThirdPartyLoginInfo>();

        /// <summary>
        /// 手机号绑定验证码的短信模板。
        /// </summary>
        [Required]
        public string BindPhoneSmsTemplate { get; set; } = "bind_phone";

        /// <summary>
        /// 短信登录验证码的短信模板。
        /// </summary>
        [Required]
        public string LoginCodeSmsTemplate { get; set; } = "sms_login";

        /// <summary>
        /// 短信登录验证码有效期（秒）。
        /// </summary>
        [Range(30, 3600)]
        public int LoginCodeExpiresInSeconds { get; set; } = 300;

        /// <summary>
        /// 同一手机号两次发送验证码的最小间隔（秒）。
        /// </summary>
        [Range(10, 3600)]
        public int LoginCodeResendIntervalSeconds { get; set; } = 60;

        /// <summary>
        /// 验证码允许的最大错误次数，超过后作废。
        /// </summary>
        [Range(1, 20)]
        public int LoginCodeMaxAttempts { get; set; } = 5;
    }
}

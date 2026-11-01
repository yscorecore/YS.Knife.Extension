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
    }
}

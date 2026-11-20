using System.ComponentModel.DataAnnotations;

namespace YS.Knife.Auth
{
    /// <summary>
    /// 登录请求。<see cref="UserName"/> 支持用户名或邮箱。
    /// </summary>
    public record LoginRequest
    {
        [Required]
        public string UserName { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;
    }
}

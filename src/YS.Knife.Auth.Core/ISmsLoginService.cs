namespace YS.Knife.Auth
{
    /// <summary>
    /// 短信验证码登录（匿名可访问）。验证码通过 <see cref="SendLoginCode"/> 下发，
    /// 再分别用 Token 或 Cookie 登录接口完成登录。
    /// </summary>
    public interface ISmsLoginService : IBaseLoginService
    {
        /// <summary>
        /// 向已绑定并确认该手机号的账号发送登录验证码。
        /// </summary>
        Task SendLoginCode(string phoneNumber, CancellationToken cancellationToken = default);
    }
}

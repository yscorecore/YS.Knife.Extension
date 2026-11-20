namespace YS.Knife.Auth
{
    /// <summary>
    /// 基于 Token（JWT）的登录服务（匿名可访问）。
    /// </summary>
    public interface ITokenLoginService : IBaseLoginService
    {
        Task<TokenInfo> Login(LoginRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// 使用手机号 + 短信验证码登录。
        /// </summary>
        Task<TokenInfo> PhoneLogin(PhoneLoginInfo info, CancellationToken cancellationToken = default);

        Task<TokenInfo> RefreshToken(string refreshToken, CancellationToken cancellationToken = default);
    }
}

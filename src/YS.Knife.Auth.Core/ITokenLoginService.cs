namespace YS.Knife.Auth
{
    /// <summary>
    /// 基于 Token（JWT）的登录服务（匿名可访问）。
    /// </summary>
    public interface ITokenLoginService
    {
        Task<TokenInfo> Login(LoginRequest request, CancellationToken cancellationToken = default);

        Task<TokenInfo> RefreshToken(string refreshToken, CancellationToken cancellationToken = default);
    }
}

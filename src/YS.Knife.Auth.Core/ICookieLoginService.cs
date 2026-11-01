namespace YS.Knife.Auth
{
    /// <summary>
    /// 基于 Cookie 的登录服务（匿名可访问）。
    /// </summary>
    public interface ICookieLoginService
    {
        Task CookieLogin(LoginRequest request, CancellationToken cancellationToken = default);
    }
}

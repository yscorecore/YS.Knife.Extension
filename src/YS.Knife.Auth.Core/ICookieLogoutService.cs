namespace YS.Knife.Auth
{
    /// <summary>
    /// 基于 Cookie 的登出服务（需要已登录）。
    /// </summary>
    public interface ICookieLogoutService
    {
        Task Logout(CancellationToken cancellationToken = default);
    }
}

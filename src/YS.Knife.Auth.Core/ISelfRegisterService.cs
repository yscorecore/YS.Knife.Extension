namespace YS.Knife.Auth
{
    /// <summary>
    /// 自助注册服务（匿名可访问）。
    /// </summary>
    public interface ISelfRegisterService
    {
        Task Register(RegisterUserInfo userInfo, CancellationToken cancellationToken = default);

        Task ResendConfirmationEmail(string email, CancellationToken cancellationToken = default);

        /// <summary>
        /// 确认邮箱（需要提供 GET 接口供邮件链接回调）。
        /// </summary>
        Task ConfirmEmail(ConfirmEmailInfo info, CancellationToken cancellationToken = default);
    }
}

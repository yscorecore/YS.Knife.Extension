namespace YS.Knife.Auth
{
    /// <summary>
    /// 认证邮件发送抽象。由宿主应用实现并注册（可配合 YS.Knife.Core 的 [Service] 自动注入），
    /// 用于发送邮箱确认码与密码重置码。
    /// </summary>
    public interface IIdentityEmailSender
    {
        Task SendEmailConfirmationCodeAsync(string email, string code, CancellationToken cancellationToken = default);

        Task SendPasswordResetCodeAsync(string email, string code, CancellationToken cancellationToken = default);
    }
}

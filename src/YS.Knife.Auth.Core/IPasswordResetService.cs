namespace YS.Knife.Auth
{
    /// <summary>
    /// 密码重置服务（匿名可访问）。
    /// </summary>
    public interface IPasswordResetService
    {
        Task ForgetPassword(string email, CancellationToken cancellationToken = default);

        Task ResetPassword(ResetPasswordInfo resetInfo, CancellationToken cancellationToken = default);
    }
}

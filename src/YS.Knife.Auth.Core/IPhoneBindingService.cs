namespace YS.Knife.Auth
{
    /// <summary>
    /// 手机号绑定服务（需要已登录）。短信验证码登录依赖已确认的手机号。
    /// </summary>
    public interface IPhoneBindingService
    {
        /// <summary>
        /// 向指定手机号发送绑定验证码。手机号已被其他账号占用时直接报错。
        /// </summary>
        Task SendBindCode(string phoneNumber, CancellationToken cancellationToken = default);

        /// <summary>
        /// 使用验证码完成手机号绑定，成功后手机号视为已确认。
        /// </summary>
        Task BindPhone(BindPhoneInfo info, CancellationToken cancellationToken = default);
    }
}

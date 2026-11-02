namespace YS.Knife.Auth
{
    /// <summary>
    /// 登录基础服务（匿名可访问）：返回登录页渲染与后续流程所需的信息。
    /// </summary>
    public interface IBaseLoginService
    {
        /// <summary>
        /// 查询登录信息。
        /// </summary>
        /// <param name="userName">可选。传入用户名或邮箱后，返回该账号是否需要进行二次验证。</param>
        Task<LoginInfo> QueryLogin(string? userName = null, CancellationToken cancellationToken = default);
    }
}

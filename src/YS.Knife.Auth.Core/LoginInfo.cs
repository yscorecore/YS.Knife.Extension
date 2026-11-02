namespace YS.Knife.Auth
{
    /// <summary>
    /// 登录页所需的登录信息。
    /// </summary>
    public record LoginInfo
    {
        /// <summary>
        /// 指定账号是否需要二次验证（双因子）。未传用户名时为 false。
        /// </summary>
        public bool RequiresTwoFactor { get; init; }

        /// <summary>
        /// 当前支持的登录方式标识，如 "password"、"third-party"。
        /// </summary>
        public string[] LoginTypes { get; init; } = Array.Empty<string>();

        /// <summary>
        /// 可用的第三方登录渠道。
        /// </summary>
        public ThirdPartyLoginInfo[] ThirdPartyLogins { get; init; } = Array.Empty<ThirdPartyLoginInfo>();

        /// <summary>
        /// 图形验证码地址。为空表示当前登录不需要图形验证码。
        /// </summary>
        public string? CaptchaUrl { get; init; }
    }
}

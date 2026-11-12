using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using YS.Knife.Sms;

namespace YS.Knife.Auth
{
    /// <summary>
    /// 基于 ASP.NET Core Identity 的统一认证服务，同时提供 Cookie 登录/登出、
    /// 自助注册、密码重置与 Token（JWT）登录能力。
    /// <para>
    /// 错误消息支持多语言：方法上 <code>[CodeException]</code> 的文本作为默认（英文）消息，
    /// 可通过同目录下的 <code>AuthService.resx</code>（默认/英文）与
    /// <code>AuthService.zh-CN.resx</code>（中文）资源文件按方法名覆盖。
    /// 资源查找依赖 <see cref="System.Globalization.CultureInfo.CurrentUICulture"/>，
    /// 宿主需启用请求本地化（如 <c>app.UseRequestLocalization()</c>）。
    /// </para>
    /// </summary>
    [Service(typeof(ICookieLoginService), Lifetime = ServiceLifetime.Scoped)]
    [Service(typeof(ICookieLogoutService), Lifetime = ServiceLifetime.Scoped)]
    [Service(typeof(ISelfRegisterService), Lifetime = ServiceLifetime.Scoped)]
    [Service(typeof(IPasswordResetService), Lifetime = ServiceLifetime.Scoped)]
    [Service(typeof(ITokenLoginService), Lifetime = ServiceLifetime.Scoped)]
    [Service(typeof(IBaseLoginService), Lifetime = ServiceLifetime.Scoped)]
    [Service(typeof(ISmsLoginService), Lifetime = ServiceLifetime.Scoped)]
    [Service(typeof(IPhoneBindingService), Lifetime = ServiceLifetime.Scoped)]
    [AutoConstructor]
    [CodeExceptions]
    public partial class AuthService : ICookieLoginService, ICookieLogoutService, ISelfRegisterService,
        IPasswordResetService, ITokenLoginService, ISmsLoginService, IPhoneBindingService
    {
        private const string AccessTokenType = "access";
        private const string RefreshTokenType = "refresh";
        private const string TokenTypeClaim = "token_type";
        private const string LoginProvider = "Knife";
        private const string RefreshTokenName = "RefreshToken";
        private const string LoginTypePassword = "password";
        private const string LoginTypeSms = "sms";
        private const string LoginTypeThirdParty = "third-party";
        private const string LoginCodeKeyPrefix = "auth:sms-login:code:";
        private const string LoginCodeCooldownKeyPrefix = "auth:sms-login:cooldown:";

        private static readonly JwtSecurityTokenHandler TokenHandler = new() { MapInboundClaims = false };

        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly IEnumerable<IIdentityEmailSender> emailSenders;
        private readonly IEnumerable<ISmsService> smsServices;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IDistributedCache cache;
        private readonly IOptions<AuthIdentityOptions> options;

        // ---------------- 错误定义（消息可被 resx 覆盖以支持多语言） ----------------

        [CodeException("AUTH001", "Invalid username or password.")]
        private partial Exception InvalidCredentials();

        [CodeException("AUTH002", "Account is locked out. Please try again later.")]
        private partial Exception AccountLockedOut();

        [CodeException("AUTH003", "Email '{email}' is already registered.")]
        private partial Exception EmailAlreadyRegistered(string email);

        [CodeException("AUTH004", "Registration failed: {errors}")]
        private partial Exception RegistrationFailed(string errors);

        [CodeException("AUTH005", "Invalid or expired confirmation code.")]
        private partial Exception InvalidConfirmationCode();

        [CodeException("AUTH006", "Invalid or expired reset code.")]
        private partial Exception InvalidResetCode();

        [CodeException("AUTH007", "No IIdentityEmailSender implementation is registered.")]
        private partial Exception EmailSenderNotConfigured();

        [CodeException("AUTH008", "Invalid or expired refresh token.")]
        private partial Exception InvalidRefreshToken();

        [CodeException("AUTH009", "Account is not allowed to sign in.")]
        private partial Exception AccountNotAllowed();

        [CodeException("AUTH010", "AuthIdentityOptions.SecretKey must be at least 32 characters long.")]
        private partial Exception InvalidSecretKey();

        [CodeException("AUTH011", "Password reset failed: {errors}")]
        private partial Exception ResetPasswordFailed(string errors);

        [CodeException("AUTH012", "Invalid or expired phone verification code.")]
        private partial Exception InvalidPhoneCode();

        [CodeException("AUTH013", "Please bind and confirm a phone number first.")]
        private partial Exception PhoneNotConfirmed();

        [CodeException("AUTH014", "Phone number '{phone}' is already bound to another account.")]
        private partial Exception PhoneAlreadyBound(string phone);

        [CodeException("AUTH015", "No ISmsService implementation is registered.")]
        private partial Exception SmsServiceNotConfigured();

        [CodeException("AUTH016", "Current user is not authenticated.")]
        private partial Exception NotAuthenticated();

        [CodeException("AUTH017", "Operation failed: {errors}")]
        private partial Exception OperationFailed(string errors);

        [CodeException("AUTH018", "This account has two-factor authentication enabled. Please sign in with your password.")]
        private partial Exception SmsLoginDisabled();

        [CodeException("AUTH019", "Verification code was sent too frequently. Please try again later.")]
        private partial Exception SendCodeTooFrequent();

        // ---------------- 登录信息 ----------------

        public async Task<LoginInfo> QueryLogin(string? userName = null, CancellationToken cancellationToken = default)
        {
            var opt = options.Value;
            var loginTypes = new List<string> { LoginTypePassword };
            if (smsServices.Any()) loginTypes.Add(LoginTypeSms);
            if (opt.ThirdPartyLogins.Length > 0) loginTypes.Add(LoginTypeThirdParty);

            // 只有指定账号时才需要判断该账号是否开启双因子
            var requiresTwoFactor = false;
            if (!string.IsNullOrWhiteSpace(userName))
            {
                var user = (await userManager.FindByNameAsync(userName))
                    ?? (await userManager.FindByEmailAsync(userName));
                requiresTwoFactor = user?.TwoFactorEnabled == true;
            }

            return new LoginInfo
            {
                RequiresTwoFactor = requiresTwoFactor,
                LoginTypes = loginTypes.ToArray(),
                ThirdPartyLogins = opt.ThirdPartyLogins,
                CaptchaUrl = opt.CaptchaUrl,
            };
        }

        // ---------------- 手机号绑定 ----------------

        public async Task SendBindCode(string phoneNumber, CancellationToken cancellationToken = default)
        {
            var user = await GetCurrentUserAsync();
            await EnsurePhoneAvailableAsync(phoneNumber, user.Id);

            var code = await userManager.GenerateChangePhoneNumberTokenAsync(user, phoneNumber);
            await GetSmsService().SendTemplate(phoneNumber, options.Value.BindPhoneSmsTemplate,
                new Dictionary<string, object> { ["code"] = code }, cancellationToken);
        }

        public async Task BindPhone(BindPhoneInfo info, CancellationToken cancellationToken = default)
        {
            var user = await GetCurrentUserAsync();
            await EnsurePhoneAvailableAsync(info.PhoneNumber, user.Id);

            // 验证通过后 Identity 会同步设置 PhoneNumberConfirmed = true
            var result = await userManager.ChangePhoneNumberAsync(user, info.PhoneNumber, info.Code);
            if (!result.Succeeded)
            {
                if (result.Errors.Any(e => e.Code == "InvalidToken")) throw InvalidPhoneCode();
                throw OperationFailed(JoinErrors(result));
            }
        }

        // ---------------- 短信验证码登录 ----------------

        public async Task SendLoginCode(string phoneNumber, CancellationToken cancellationToken = default)
        {
            var user = await FindConfirmedPhoneUserAsync(phoneNumber);
            // 防止账号枚举：手机号未绑定任何账号时静默返回
            if (user is null) return;

            // 已开启双因子的账号禁止短信免密登录，必须走密码 + 双因子流程
            if (user.TwoFactorEnabled) throw SmsLoginDisabled();

            await EnsureNotTooFrequentAsync(phoneNumber);

            var opt = options.Value;
            var code = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
            var record = new SmsLoginCode
            {
                Code = code,
                ExpiresAtUtc = DateTime.UtcNow.AddSeconds(opt.LoginCodeExpiresInSeconds),
                Attempts = 0,
            };
            await SaveLoginCodeAsync(phoneNumber, record, opt.LoginCodeExpiresInSeconds);

            await GetSmsService().SendTemplate(phoneNumber, opt.LoginCodeSmsTemplate,
                new Dictionary<string, object> { ["code"] = code }, cancellationToken);
        }

        public async Task<TokenInfo> PhoneLogin(PhoneLoginInfo info, CancellationToken cancellationToken = default)
        {
            var user = await VerifyLoginCodeAsync(info);
            return await IssueTokenPairAsync(user);
        }

        public async Task CookiePhoneLogin(PhoneLoginInfo info, CancellationToken cancellationToken = default)
        {
            var user = await VerifyLoginCodeAsync(info);
            await signInManager.SignInAsync(user, isPersistent: true);
        }

        private async Task<IdentityUser> VerifyLoginCodeAsync(PhoneLoginInfo info)
        {
            var user = await FindConfirmedPhoneUserAsync(info.PhoneNumber) ?? throw InvalidPhoneCode();
            if (user.TwoFactorEnabled) throw SmsLoginDisabled();

            var record = await LoadLoginCodeAsync(info.PhoneNumber);
            if (record is null || DateTime.UtcNow > record.ExpiresAtUtc) throw InvalidPhoneCode();

            if (!CodeEquals(record.Code, info.Code))
            {
                record.Attempts++;
                if (record.Attempts >= options.Value.LoginCodeMaxAttempts)
                    await cache.RemoveAsync(LoginCodeKeyPrefix + info.PhoneNumber);
                else
                    await SaveLoginCodeAsync(info.PhoneNumber, record, RemainingSeconds(record));
                throw InvalidPhoneCode();
            }

            // 验证码单次有效
            await cache.RemoveAsync(LoginCodeKeyPrefix + info.PhoneNumber);
            return user;
        }

        // ---------------- Cookie 登录 / 登出 ----------------

        public async Task CookieLogin(LoginRequest request, CancellationToken cancellationToken = default)
        {
            var user = await ValidateCredentialsAsync(request);
            await signInManager.SignInAsync(user, isPersistent: true);
        }

        public Task Logout(CancellationToken cancellationToken = default)
        {
            return signInManager.SignOutAsync();
        }

        // ---------------- 自助注册 ----------------

        public async Task Register(RegisterUserInfo userInfo, CancellationToken cancellationToken = default)
        {
            if (await userManager.FindByEmailAsync(userInfo.Email) is not null)
                throw EmailAlreadyRegistered(userInfo.Email);

            var user = new IdentityUser { UserName = userInfo.Email, Email = userInfo.Email };
            var result = await userManager.CreateAsync(user, userInfo.Password);
            if (!result.Succeeded)
                throw RegistrationFailed(JoinErrors(result));

            var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
            await GetEmailSender().SendEmailConfirmationCodeAsync(userInfo.Email, code, cancellationToken);
        }

        public async Task ResendConfirmationEmail(string email, CancellationToken cancellationToken = default)
        {
            var user = await userManager.FindByEmailAsync(email);
            // 防止账号枚举：用户不存在或已确认时静默返回
            if (user is null || user.EmailConfirmed) return;

            var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
            await GetEmailSender().SendEmailConfirmationCodeAsync(email, code, cancellationToken);
        }

        public async Task ConfirmEmail(ConfirmEmailInfo info, CancellationToken cancellationToken = default)
        {
            var user = await userManager.FindByEmailAsync(info.Email)
                ?? throw InvalidConfirmationCode();

            var result = await userManager.ConfirmEmailAsync(user, info.Code);
            if (!result.Succeeded) throw InvalidConfirmationCode();
        }

        // ---------------- 密码重置 ----------------

        public async Task ForgetPassword(string email, CancellationToken cancellationToken = default)
        {
            var user = await userManager.FindByEmailAsync(email);
            // 防止账号枚举：用户不存在时静默返回
            if (user is null) return;

            var code = await userManager.GeneratePasswordResetTokenAsync(user);
            await GetEmailSender().SendPasswordResetCodeAsync(email, code, cancellationToken);
        }

        public async Task ResetPassword(ResetPasswordInfo resetInfo, CancellationToken cancellationToken = default)
        {
            var user = await userManager.FindByEmailAsync(resetInfo.Email)
                ?? throw InvalidResetCode();

            var result = await userManager.ResetPasswordAsync(user, resetInfo.ResetCode, resetInfo.Password);
            if (!result.Succeeded)
            {
                if (result.Errors.Any(e => e.Code == "InvalidToken")) throw InvalidResetCode();
                throw ResetPasswordFailed(JoinErrors(result));
            }
        }

        // ---------------- Token（JWT）登录 ----------------

        public async Task<TokenInfo> Login(LoginRequest request, CancellationToken cancellationToken = default)
        {
            var user = await ValidateCredentialsAsync(request);
            return await IssueTokenPairAsync(user);
        }

        public async Task<TokenInfo> RefreshToken(string refreshToken, CancellationToken cancellationToken = default)
        {
            var userId = ValidateRefreshToken(refreshToken);
            var user = await userManager.FindByIdAsync(userId) ?? throw InvalidRefreshToken();

            // 与持久化的刷新令牌比对（单次使用，签发新令牌时轮换）
            var stored = await userManager.GetAuthenticationTokenAsync(user, LoginProvider, RefreshTokenName);
            if (!string.Equals(stored, refreshToken, StringComparison.Ordinal)) throw InvalidRefreshToken();

            return await IssueTokenPairAsync(user);
        }

        // ---------------- 内部实现 ----------------

        private async Task<IdentityUser> ValidateCredentialsAsync(LoginRequest request)
        {
            // 用户名未命中时依次按邮箱、已确认手机号查找
            var user = (await userManager.FindByNameAsync(request.UserName))
                ?? (await userManager.FindByEmailAsync(request.UserName))
                ?? await FindConfirmedPhoneUserAsync(request.UserName)
                ?? throw InvalidCredentials();

            var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
            if (result.IsLockedOut) throw AccountLockedOut();
            if (result.IsNotAllowed) throw AccountNotAllowed();
            if (!result.Succeeded) throw InvalidCredentials();

            return user;
        }

        private IIdentityEmailSender GetEmailSender()
            => emailSenders.FirstOrDefault() ?? throw EmailSenderNotConfigured();

        private ISmsService GetSmsService()
            => smsServices.FirstOrDefault() ?? throw SmsServiceNotConfigured();

        private Task<IdentityUser?> FindConfirmedPhoneUserAsync(string phoneNumber)
            => userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber && u.PhoneNumberConfirmed);

        // 手机号全局唯一：拒绝绑定已被其他账号占用的号码
        private async Task EnsurePhoneAvailableAsync(string phoneNumber, string currentUserId)
        {
            var used = await userManager.Users.AnyAsync(u => u.PhoneNumber == phoneNumber && u.Id != currentUserId);
            if (used) throw PhoneAlreadyBound(phoneNumber);
        }

        private async Task<IdentityUser> GetCurrentUserAsync()
        {
            var principal = httpContextAccessor.HttpContext?.User ?? throw NotAuthenticated();
            var userId = userManager.GetUserId(principal) ?? throw NotAuthenticated();
            return await userManager.FindByIdAsync(userId) ?? throw NotAuthenticated();
        }

        // 同一手机号在冷却时间内不允许重复发送
        private async Task EnsureNotTooFrequentAsync(string phoneNumber)
        {
            var key = LoginCodeCooldownKeyPrefix + phoneNumber;
            if (await cache.GetStringAsync(key) is not null) throw SendCodeTooFrequent();
            await cache.SetStringAsync(key, "1",
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(options.Value.LoginCodeResendIntervalSeconds),
                });
        }

        private Task SaveLoginCodeAsync(string phoneNumber, SmsLoginCode record, int lifetimeSeconds)
            => cache.SetStringAsync(LoginCodeKeyPrefix + phoneNumber, JsonSerializer.Serialize(record),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(Math.Max(1, lifetimeSeconds)),
                });

        private async Task<SmsLoginCode?> LoadLoginCodeAsync(string phoneNumber)
        {
            var content = await cache.GetStringAsync(LoginCodeKeyPrefix + phoneNumber);
            return string.IsNullOrEmpty(content) ? null : JsonSerializer.Deserialize<SmsLoginCode>(content);
        }

        private static int RemainingSeconds(SmsLoginCode record)
            => (int)(record.ExpiresAtUtc - DateTime.UtcNow).TotalSeconds;

        private static bool CodeEquals(string expected, string actual)
            => CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(expected), Encoding.UTF8.GetBytes(actual));

        private static string JoinErrors(IdentityResult result)
            => string.Join(" ", result.Errors.Select(e => e.Description));

        private async Task<TokenInfo> IssueTokenPairAsync(IdentityUser user)
        {
            var opt = options.Value;
            var utcNow = DateTime.UtcNow;

            var roles = userManager.SupportsUserRole
                ? await userManager.GetRolesAsync(user)
                : (IList<string>)Array.Empty<string>();

            var accessToken = CreateJwt(user, roles, utcNow.AddMinutes(opt.AccessTokenExpiresInMinutes), AccessTokenType);
            var refreshToken = CreateJwt(user, Array.Empty<string>(), utcNow.AddDays(opt.RefreshTokenExpiresInDays), RefreshTokenType);

            await userManager.SetAuthenticationTokenAsync(user, LoginProvider, RefreshTokenName, refreshToken);

            return new TokenInfo("Bearer", accessToken, opt.AccessTokenExpiresInMinutes * 60L, refreshToken);
        }

        private string CreateJwt(IdentityUser user, IList<string> roles, DateTime expires, string tokenType)
        {
            var opt = options.Value;
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
                new(TokenTypeClaim, tokenType),
            };

            if (tokenType == AccessTokenType)
            {
                claims.Add(new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName ?? string.Empty));
                if (!string.IsNullOrEmpty(user.Email))
                    claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
                claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
            }

            var token = new JwtSecurityToken(
                issuer: opt.Issuer,
                audience: opt.Audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expires,
                signingCredentials: new SigningCredentials(GetSigningKey(), SecurityAlgorithms.HmacSha256));
            return TokenHandler.WriteToken(token);
        }

        private string ValidateRefreshToken(string refreshToken)
        {
            var opt = options.Value;
            try
            {
                var principal = TokenHandler.ValidateToken(refreshToken, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = opt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = opt.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1),
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = GetSigningKey(),
                }, out _);

                if (principal.FindFirst(TokenTypeClaim)?.Value != RefreshTokenType)
                    throw InvalidRefreshToken();

                return principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? throw InvalidRefreshToken();
            }
            catch (Exception ex) when (ex is SecurityTokenException or ArgumentException)
            {
                throw InvalidRefreshToken();
            }
        }

        private SymmetricSecurityKey GetSigningKey()
        {
            var secretKey = options.Value.SecretKey;
            if (string.IsNullOrEmpty(secretKey) || Encoding.UTF8.GetByteCount(secretKey) < 32)
                throw InvalidSecretKey();
            return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        }

        // 短信登录验证码记录（存于分布式缓存，随缓存过期自动清理）
        private sealed class SmsLoginCode
        {
            public string Code { get; set; } = null!;

            public DateTime ExpiresAtUtc { get; set; }

            public int Attempts { get; set; }
        }
    }
}

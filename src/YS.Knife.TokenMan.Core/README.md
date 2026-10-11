# YS.Knife.TokenMan.Core

第三方平台 AccessToken 管理的抽象层。定义了带缓存、过期缓冲与提前预加载能力的 `ITokenManager` 接口。

## 主要功能

- `ITokenManager.GetToken<T>`：
  - Token 不存在或已过期时调用 `tokenFactory` 获取新 Token，按 `expiredTimeFunc` 计算的绝对过期时间写入缓存
  - `bufferSeconds`：过期缓冲（默认 300 秒），提前按缓冲时间过期，避免网络延迟/时钟偏差导致拿到失效 Token
  - `preloadSeconds`：提前预加载（默认 60 秒），高并发下在 Token 失效前由一个请求刷新，其余请求继续使用旧 Token
- `ITokenManagerExtensions` 扩展重载：过期时间支持 `DateTimeOffset`、`TimeSpan`（相对时长）、`DateTime` 三种形式

## 接口定义

```csharp
public interface ITokenManager
{
    Task<T> GetToken<T>(
        string name,
        Func<CancellationToken, Task<T>> tokenFactory,
        Func<T, DateTimeOffset> expiredTimeFunc,
        int bufferSeconds = 300,
        int preloadSeconds = 60,
        CancellationToken token = default) where T : class;
}
```

## 使用示例

```csharp
public class MyService
{
    private readonly ITokenManager tokenManager;

    public MyService(ITokenManager tokenManager) => this.tokenManager = tokenManager;

    public async Task<string> GetAccessToken()
    {
        var token = await tokenManager.GetToken<AccessTokenResponse>(
            name: "MyApi:AccessToken",
            tokenFactory: ct => httpClient.RequestNewToken(ct),
            expiredTimeFunc: t => TimeSpan.FromSeconds(t.ExpiresIn)); // 扩展重载
        return token.AccessToken;
    }
}
```

## 依赖说明

本包无任何外部依赖，仅包含接口定义。默认实现见 `YS.Knife.TokenMan.Impl.Default`（基于分布式缓存 + 分布式锁）。典型使用者如 `YS.Knife.Wechat.MiniProgram` 的 `WechatTokenManager`。

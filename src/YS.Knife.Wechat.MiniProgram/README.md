# YS.Knife.Wechat.MiniProgram

微信小程序服务端 API 封装，包含登录凭证校验、AccessToken 管理、小程序码生成、URL Link 生成与订阅消息发送。

## 主要功能

- `IWechatService` / `WechatService`：
  - `Code2Session(jscode)`：登录凭证校验（`sns/jscode2session`），返回 OpenId / SessionKey / UnionId
  - `GetUnlimited(scene, page, checkPage, isHyaline)`：获取不限制数量的小程序码（`wxa/getwxacodeunlimit`），返回图片流
  - `GenerateUrlLink(request)`：生成小程序 URL Link（`wxa/generate_urllink`）
  - `NotificationMessage<T>(body)`：发送订阅消息（`cgi-bin/message/subscribe/send`）
- `WechatTokenManager`：基于 `ITokenManager`（YS.Knife.TokenMan）缓存 AccessToken，缓冲 120 秒、提前 30 秒预加载
- `WechatClient`：底层 HTTP 客户端（`HttpClient`），统一处理微信错误码并抛出 `WechatException`（含 `ErrorCode`）
- 模型：`Code2SessionResponse`、`GetAccessTokenResponse`、`GenerateUrlLinkRequest/Response`、`GetWxacodeunlimitRequest`、`WechatNotificationMessage<T>` 等

## 快速开始

1. 配置 `WechatOptions`（`appsettings.json` 中 `Wechat` 节）：

```json
{
  "Wechat": {
    "AppId": "wx****",
    "AppSecret": "****",
    "Env": "release",
    "MiniprogramState": "release"
  }
}
```

2. 确保已注册 `ITokenManager`（如引用 `YS.Knife.TokenMan.Impl.Default` 及其缓存/锁实现）。服务与 `HttpClient` 通过 YS.Knife 自动注册。

3. 使用：

```csharp
public class LoginService
{
    private readonly IWechatService wechat;

    public LoginService(IWechatService wechat) => this.wechat = wechat;

    public async Task<string> Login(string jsCode)
    {
        var session = await wechat.Code2Session(jsCode);
        return session.OpenId;
    }

    public Task<Stream> CreateQrCode(string scene)
        => wechat.GetUnlimited(scene, page: "pages/index/index");
}
```

## 依赖说明

- `YS.Knife.TokenMan.Core`：AccessToken 缓存抽象。
- `YS.Knife.Extensions.HttpClient`：`GetAsObject` / `PostAsObject` / `SendAsResponse` 等 HTTP 扩展。
- `Microsoft.Extensions.Http`（6.x+）、`YS.Knife.Core`（2.0.2）。

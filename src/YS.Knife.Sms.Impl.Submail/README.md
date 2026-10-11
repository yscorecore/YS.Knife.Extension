# YS.Knife.Sms.Impl.Submail

`YS.Knife.Sms` 短信抽象的赛邮（Submail）云通信实现，通过 Submail 短信模板接口（`sms/xsend`）发送短信。

## 主要功能

- 提供 `SmsService`，实现 `ISmsService` 接口，调用 Submail API v4 发送模板短信
- `SubmailOptions`：配置 AppId / AppKey（`[Options]` 特性，支持从配置文件绑定）
- 响应状态校验：非 `success` 时抛出 `SubmailException`，包含错误码与错误信息
- 通过 `[Service(Lifetime = Singleton)]` 特性自动注册到 DI 容器

## 配置

在 `appsettings.json` 中配置（配置节名与类型名一致）：

```json
{
  "SubmailOptions": {
    "AppId": "your-app-id",
    "AppKey": "your-app-key"
  }
}
```

## 使用示例

```csharp
public class MyService
{
    private readonly ISmsService smsService;

    public MyService(ISmsService smsService)
    {
        this.smsService = smsService;
    }

    public async Task SendAsync()
    {
        try
        {
            // Template 对应 Submail 后台的短信模板项目标识（project）
            await smsService.SendSms("13800000000", "your-project-id",
                new Dictionary<string, object> { ["code"] = "123456" });
        }
        catch (SubmailException ex)
        {
            // 发送失败，ex.Message 中包含 Submail 返回的 Code 与 Msg
        }
    }
}
```

## 类型说明

- `SubmailOptions`：`AppId`（应用 ID）、`AppKey`（应用密钥，作为 signature 发送）
- `SubmailBaseResponse`：响应基类，包含 `Code`、`Msg`、`Status`
- `SubMailSendSmsResponse`：发送响应，额外包含 `SendId`、`Fee`
- `BaseResponseExtensions.EnsureSuccessCode`：`Status != "success"` 时抛出 `SubmailException`

## 依赖说明

- `YS.Knife.Sms.Core`：短信抽象接口
- `YS.Knife.Extensions.HttpClient`：提供 `HttpClient.PostAsObject` 扩展
- `YS.Knife.Extensions`：提供 `ToJsonText` 等扩展
- `YS.Knife.Core`：提供 `[Service]`、`[AutoConstructor]`、`[Options]` 源生成特性

目标框架 .NET 6+。

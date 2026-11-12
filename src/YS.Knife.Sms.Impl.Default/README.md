# YS.Knife.Sms.Impl.Default

`YS.Knife.Sms` 的默认实现，把业务层的 `SendTemplate` 请求按模板名中的渠道标识路由到已注册的 `ISmsProvider`。

## 主要功能

- `DefaultSmsService`：实现 `ISmsService.SendTemplate`，向具体的短信渠道发起发送
- 模板寻址格式：`template@provider`（如 `verify_code@submail`）
  - 指定 `provider` 时，按渠道 `Name` 匹配（忽略大小写），找不到时抛出异常
  - 未指定 `provider` 时，仅注册了一个渠道则自动使用；注册多个渠道时抛出异常；未注册任何渠道时抛出异常
- 通过 `[Service]` / `[AutoConstructor]` 特性自动注册到 DI 容器，构造函数注入 `IEnumerable<ISmsProvider>`

## 快速开始

引用本包以及至少一个渠道实现（如 `YS.Knife.Sms.Provider.Submail`），然后注入 `ISmsService`：

```csharp
public class MyService
{
    private readonly ISmsService smsService;

    public MyService(ISmsService smsService)
    {
        this.smsService = smsService;
    }

    public Task SendCodeAsync(string phone, string code, CancellationToken cancellationToken)
    {
        return smsService.SendTemplate(phone, "verify_code@submail",
            new Dictionary<string, object> { ["code"] = code }, cancellationToken);
    }
}
```

## 依赖说明

- `YS.Knife.Sms.Core`：`ISmsService`、`ISmsProvider`、`SmsInfo` 抽象
- `YS.Knife.Core`：`[Service]`、`[AutoConstructor]` 源生成特性

目标框架 .NET 6+。

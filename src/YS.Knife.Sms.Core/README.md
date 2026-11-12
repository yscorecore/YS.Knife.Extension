# YS.Knife.Sms.Core

短信服务抽象层，定义面向业务的短信发送接口 `ISmsService`、面向渠道实现的 `ISmsProvider`，以及短信信息模型。

## 主要功能

- `ISmsService`：业务层发送模板短信的统一入口 `SendTemplate`
- `ISmsProvider`：短信渠道抽象，通过 `Name` 标识渠道，由 `SendSms` 完成实际发送
- `SmsInfo`：短信信息记录（手机号、模板、模板参数）
- `SmsTemplateInfo`：短信模板描述（渠道名、模板名）
- `SmsServiceExtensions`：为 `ISmsProvider` 提供便捷重载，免去手动构造 `SmsInfo`

## 核心定义

```csharp
public interface ISmsService
{
    Task SendTemplate(string phone, string template, Dictionary<string, object> args, CancellationToken cancellationToken);
}

public interface ISmsProvider
{
    string Name { get; }
    Task SendSms(SmsInfo smsInfo);
}

public record SmsInfo
{
    public string Phone { get; set; }                    // 手机号
    public string Template { get; set; }                 // 模板标识
    public Dictionary<string, object> Args { get; set; } // 模板变量
}
```

## 模板寻址与渠道选择

`SendTemplate` 的 `template` 参数由默认实现（`YS.Knife.Sms.Impl.Default`）解析，约定格式为 `template@provider`：

- `verify_code@submail`：显式指定使用 `Name` 为 `submail` 的渠道
- `verify_code`：不指定渠道，仅注册了一个渠道时自动使用；注册了多个渠道时抛出异常，未注册任何渠道时同样抛出异常

## 使用示例

通过依赖注入获取 `ISmsService`（需引用实现包，如 `YS.Knife.Sms.Impl.Default` 与具体渠道包）：

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
        // 指定渠道：模板名@渠道名
        return smsService.SendTemplate(phone, "verify_code@submail",
            new Dictionary<string, object> { ["code"] = code }, cancellationToken);
    }
}
```

## 依赖说明

本包无外部依赖，仅包含接口与模型定义。运行时需配合实现包使用：

- `YS.Knife.Sms.Impl.Default`：`ISmsService` 的默认实现，按模板名路由到已注册的 `ISmsProvider`
- `YS.Knife.Sms.Provider.Submail`：基于赛邮（Submail）云通信的渠道实现

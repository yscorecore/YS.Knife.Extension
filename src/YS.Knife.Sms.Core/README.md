# YS.Knife.Sms.Core

短信服务抽象层，定义统一的短信发送接口与短信信息模型，供各短信渠道实现（如 Submail）使用。

## 主要功能

- `ISmsService`：短信发送服务接口
- `SmsInfo`：短信信息记录（手机号、模板、模板参数）
- `SmsServiceExtensions`：提供便捷的重载方法，免去手动构造 `SmsInfo`

## 核心定义

```csharp
public interface ISmsService
{
    Task SendSms(SmsInfo smsInfo);
}

public record SmsInfo
{
    public string Phone { get; set; }                    // 手机号
    public string Template { get; set; }                 // 模板标识
    public IDictionary<string, object> Args { get; set; } // 模板变量
}
```

## 使用示例

通过依赖注入获取 `ISmsService`（需配合实现包，如 `YS.Knife.Sms.Impl.Submail`）：

```csharp
public class MyService
{
    private readonly ISmsService smsService;

    public MyService(ISmsService smsService)
    {
        this.smsService = smsService;
    }

    public async Task SendCodeAsync(string phone, string code)
    {
        // 方式一：使用扩展方法
        await smsService.SendSms(phone, "verify_code",
            new Dictionary<string, object> { ["code"] = code });

        // 方式二：手动构造 SmsInfo
        await smsService.SendSms(new SmsInfo
        {
            Phone = phone,
            Template = "verify_code",
            Args = new Dictionary<string, object> { ["code"] = code }
        });
    }
}
```

## 依赖说明

本包无外部依赖，仅包含接口与模型定义。运行时需配合实现包使用：

- `YS.Knife.Sms.Impl.Submail`：基于赛邮（Submail）云通信的短信发送实现

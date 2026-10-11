# YS.Knife.Webhooks.Impl.Default

`IWebHookDeliveryService` 的默认实现，通过 HTTP POST 将 Webhook 事件 JSON 发送到回调地址，并附带 HMAC-SHA256 签名。

## 主要功能

- `WebHookDeliveryService`：事件投递实现
  - 将事件（或仅数据负载）序列化为 JSON，以 POST 方式发送到 `WebhookConfig.CallbackUrl`
  - 自动附加请求头：
    - `X-Webhook-EventId`：事件 ID
    - `X-Webhook-EventType`：事件类型
    - `X-Webhook-EventTime`：事件时间戳（毫秒）
    - `X-Webhook-Signature`：对 `{时间戳}.{请求体}` 计算的 HMAC-SHA256 签名（十六进制小写）
  - 投递失败时抛出 `WebhookDeliveryException`，包含状态码、响应内容、事件 ID 等信息
- 通过 FlyTiger `[Service]` 特性自动注册到 DI 容器

## 快速开始

```csharp
using YS.Knife.WebHooks;

public class NotifyService
{
    private readonly IWebHookDeliveryService deliveryService;

    public NotifyService(IWebHookDeliveryService deliveryService)
    {
        this.deliveryService = deliveryService;
    }

    public async Task NotifyAsync(CancellationToken token)
    {
        var evt = new WebhookEvent<MyPayload>(
            Guid.NewGuid().ToString("N"), "order.paid", DateTimeOffset.Now, new MyPayload());

        var config = new WebhookConfig
        {
            CallbackUrl = "https://example.com/webhook",
            SecretKey = "shared-secret",
            Encoding = System.Text.Encoding.UTF8,   // 可选，默认 UTF8
            JsonOptions = null                       // 可选，自定义 JSON 序列化
        };

        try
        {
            await deliveryService.SendEvent(evt, config, token);
        }
        catch (WebhookDeliveryException ex)
        {
            // ex.EventId / ex.Url / ex.StatusCode / ex.ResponseContent
            Console.WriteLine($"投递失败：{ex.StatusCode} {ex.ResponseContent}");
        }
    }
}
```

## 接收方验签

接收方使用共享密钥对 `{X-Webhook-EventTime}.{原始请求体}` 计算 HMAC-SHA256（hex 小写），与 `X-Webhook-Signature` 比对即可验证请求来源。

## 依赖说明

- 目标框架：.NET 6（`net6.0`）
- `YS.Knife.Webhooks.Core`：抽象接口与模型
- `YS.Knife.Extensions`、`YS.Knife.Core`、`FlyTiger`：HTTP 扩展与服务自动注册
- `System.Net.Http`（HttpClient）、`Microsoft.Extensions.Logging`

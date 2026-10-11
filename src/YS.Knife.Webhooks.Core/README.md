# YS.Knife.Webhooks.Core

Webhooks（回调通知）功能的抽象层，定义事件投递服务接口及事件/配置模型。

## 主要功能

- `IWebHookDeliveryService`：Webhook 事件投递服务接口
- `WebhookEvent<T>`：通用事件模型（事件 ID、类型、时间、数据负载）
- `WebhookConfig`：投递配置（回调地址、签名密钥、编码、JSON 序列化选项等）

## 快速开始

构造事件并调用投递服务（实现由 `YS.Knife.Webhooks.Impl.Default` 提供）：

```csharp
using YS.Knife.WebHooks;

public class OrderNotifier
{
    private readonly IWebHookDeliveryService deliveryService;

    public OrderNotifier(IWebHookDeliveryService deliveryService)
    {
        this.deliveryService = deliveryService;
    }

    public async Task NotifyAsync(CancellationToken token)
    {
        var evt = new WebhookEvent<OrderPayload>(
            EventId: Guid.NewGuid().ToString("N"),
            EventType: "order.created",
            EventTime: DateTimeOffset.Now,
            Data: new OrderPayload("SO-1001"));

        var config = new WebhookConfig
        {
            CallbackUrl = "https://example.com/webhook",
            SecretKey = "shared-secret",
            EventDataOnly = false   // true 时只发送 Data 负载，不包裹事件元信息
        };

        await deliveryService.SendEvent(evt, config, token);
    }
}
```

## 接口说明

```csharp
public interface IWebHookDeliveryService
{
    Task SendEvent<T>(WebhookEvent<T> webhookEvent, WebhookConfig config, CancellationToken token = default);
}
```

## 依赖说明

- 目标框架：.NET 6（`net6.0`）
- 无第三方运行时依赖，仅定义抽象契约
- 实现层请参考 `YS.Knife.Webhooks.Impl.Default`

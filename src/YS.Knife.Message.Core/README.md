# YS.Knife.Message.Core

站内消息的抽象层，定义消息查询服务接口与消息模型。

## 主要功能

- `IMessageService`：消息服务接口，提供分页查询消息能力
- `MessageInfo`：消息模型（标题、内容、发送人、发送时间）
- `MessageReplyInfo`：消息回复模型（预留）
- 查询接口标记了 `[DataSource(nameof(MessageInfo))]`，可接入 `YS.Knife.DataItem` 数据源体系，支持通用的 `LimitQueryInfo` 分页查询

## 接口定义

```csharp
public interface IMessageService
{
    [DataSource(nameof(MessageInfo))]
    Task<PagedList<MessageInfo>> GetMesssages(LimitQueryInfo queryInfo, CancellationToken cancellationToken);
}

public record MessageInfo
{
    public string Title { get; init; }
    public string Content { get; init; }
    public string From { get; init; }
    public DateTime SendTime { get; init; }
}
```

## 使用示例

```csharp
public class MyService
{
    private readonly IMessageService messageService;

    public MyService(IMessageService messageService)
    {
        this.messageService = messageService;
    }

    public async Task Demo(CancellationToken cancellationToken)
    {
        var queryInfo = new LimitQueryInfo
        {
            // 分页、过滤、排序条件
        };

        PagedList<MessageInfo> page = await messageService.GetMesssages(queryInfo, cancellationToken);
        foreach (var message in page.Items)
        {
            Console.WriteLine($"{message.SendTime}: {message.Title} - {message.Content}");
        }
    }
}
```

## 依赖说明

- `YS.Knife.DataSource.Core`：提供 `DataSourceAttribute`、`LimitQueryInfo`、`PagedList` 等查询与数据源基础设施

本包仅为抽象层，消息的实际存储与查询由具体实现项目提供。

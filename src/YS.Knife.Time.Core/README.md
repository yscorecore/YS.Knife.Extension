# YS.Knife.Time.Core

时间服务抽象层，定义统一的"当前时间"获取接口，便于在单机、数据库、Redis 等不同时间源之间切换实现。

## 主要功能

- `ITimeService`：当前时间获取接口，返回 `DateTimeOffset`
- 接口方法标注 `[DataItem]` 特性，可被数据项（DataItem）体系识别暴露

## 核心接口

```csharp
public interface ITimeService
{
    [DataItem(nameof(Now))]
    Task<DateTimeOffset> Now(CancellationToken cancellationToken = default);
}
```

## 使用示例

通过依赖注入获取 `ITimeService`（需配合任一实现包使用）：

```csharp
public class MyService
{
    private readonly ITimeService timeService;

    public MyService(ITimeService timeService)
    {
        this.timeService = timeService;
    }

    public async Task<DateTimeOffset> GetNowAsync(CancellationToken cancellationToken)
    {
        return await timeService.Now(cancellationToken);
    }
}
```

## 依赖说明

- `YS.Knife.DataItem.Core`：提供 `[DataItem]` 特性

本包只定义抽象，具体实现由以下包提供：

- `YS.Knife.Time.Impl.Default`：使用本机系统时间
- `YS.Knife.Time.Impl.DbFactory`：通过数据库 SQL 获取服务器时间
- `YS.Knife.Time.Impl.Redis`：通过 Redis `TIME` 命令获取服务器时间

在分布式场景中，使用统一时间源（数据库或 Redis）可避免各节点本机时钟不一致的问题。

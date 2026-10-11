# YS.Knife.Time.Impl.Default

`YS.Knife.Time` 时间服务的默认实现，直接返回本机系统时间。

## 主要功能

- 提供 `DefaultTimeService`，实现 `ITimeService` 接口
- `Now` 方法返回 `DateTimeOffset.Now`（本机当前时间，含时区偏移）
- 通过 `[Service(Lifetime = Singleton)]` 特性自动注册到 DI 容器

## 使用示例

引用本包并完成服务扫描注册后，直接注入 `ITimeService` 即可：

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
        // 返回本机系统时间
        return await timeService.Now(cancellationToken);
    }
}
```

## 适用场景

- 单体应用或开发环境，对时间一致性要求不高
- 无需额外的数据库或 Redis 依赖

在分布式部署、多节点时钟可能不一致的场景下，建议改用 `YS.Knife.Time.Impl.DbFactory` 或 `YS.Knife.Time.Impl.Redis`，以统一时间源。

## 依赖说明

- `YS.Knife.Time.Core`：提供 `ITimeService` 抽象
- `YS.Knife.Core`：提供 `[Service]` 自动服务注册特性

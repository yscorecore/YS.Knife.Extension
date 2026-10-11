# YS.Knife.Time.Impl.Redis

`YS.Knife.Time` 时间服务的 Redis 实现，通过 Redis `TIME` 命令获取 Redis 服务器当前时间，作为分布式系统的统一时间源。

## 主要功能

- 提供 `RedisTimeService`，实现 `ITimeService` 接口
- 每次调用创建 `ConnectionMultiplexer` 连接并执行 `TIME` 命令
- 将返回的 Unix 秒 + 微秒转换为精确的 `DateTimeOffset`
- `RedisTimeOptions` 支持字符串配置或 `ConfigurationOptions` 对象两种方式
- 通过 `[Service(Lifetime = Singleton)]` 特性自动注册到 DI 容器

## 配置

配置节名为 `Redis`：

```json
{
  "Redis": {
    "Configuration": "localhost:6379"
  }
}
```

- `Configuration`：连接字符串（默认 `localhost`）
- `ConfigurationOptions`：StackExchange.Redis 的 `ConfigurationOptions` 对象；设置后优先于 `Configuration` 生效

## 使用示例

```csharp
public class MyService
{
    private readonly ITimeService timeService;

    public MyService(ITimeService timeService)
    {
        this.timeService = timeService;
    }

    public async Task<DateTimeOffset> GetRedisNowAsync(CancellationToken cancellationToken)
    {
        // 返回 Redis 服务器当前时间（UTC）
        return await timeService.Now(cancellationToken);
    }
}
```

## 适用场景

多节点部署且已有 Redis 基础设施时，以 Redis 服务器时间作为统一时间基准，避免各应用节点本机时钟漂移。

## 依赖说明

- `YS.Knife.Time.Core`：提供 `ITimeService` 抽象
- `YS.Knife.Core`：提供 `[Service]`、`[AutoConstructor]`、`[Options]` 源生成特性
- `StackExchange.Redis`（2.*）

仓库内的 `YS.Knife.Time.Impl.Redis.UnitTest` 项目附带 docker-compose，可用于本地集成测试。

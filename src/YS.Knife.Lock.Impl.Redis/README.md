# YS.Knife.Lock.Impl.Redis

基于 Redis（StackExchange.Redis）的分布式 `ILockService` 实现，适用于多实例集群部署。

## 主要功能

- 实现 `YS.Knife.Lock.Core` 定义的 `ILockService` 接口
- 使用 Redis 原生命令实现：`LockTakeAsync`（加锁）、`LockReleaseAsync`（解锁）、`LockExtendAsync`（续期）、`LockQueryAsync`（查询）
- 支持自定义锁 key 前缀与 Redis 连接配置
- 自动注册为单例服务，内部延迟创建 `ConnectionMultiplexer` 并在释放时正确销毁

## 快速开始

引用本包后 `RedisLockService` 会自动注册为 `ILockService` 的实现。通过配置节 `Redis` 配置连接（`RedisLockOptions` 由 `[Options("Redis")]` 特性绑定）：

```json
{
  "Redis": {
    "Configuration": "localhost:6379",
    "LockKeyPrefix": "Lock_"
  }
}
```

也可以通过代码配置：

```csharp
services.Configure<RedisLockOptions>(options =>
{
    options.Configuration = "localhost:6379";
    options.LockKeyPrefix = "Lock_";
    // 或使用 ConfigurationOptions 进行更精细的连接配置
    // options.ConfigurationOptions = new ConfigurationOptions { ... };
});
```

业务代码中注入 `ILockService` 使用：

```csharp
public class MyService
{
    private readonly ILockService lockService;

    public MyService(ILockService lockService)
    {
        this.lockService = lockService;
    }

    public async Task Demo()
    {
        // 分布式加锁
        var locked = await lockService.Lock("order-1001", TimeSpan.FromHours(1));

        // 在锁保护下执行，结束后自动解锁
        await lockService.RunWithLock("order-1001", () =>
        {
            // 临界区代码
        });

        // 全局只执行一次
        await lockService.GlobalRunOnce("init-flag", () =>
        {
            // 集群范围内仅执行一次
        });
    }
}
```

## 配置项（RedisLockOptions）

| 配置项 | 说明 | 默认值 |
| --- | --- | --- |
| `Configuration` | Redis 连接字符串 | `localhost` |
| `LockKeyPrefix` | 锁 key 的前缀 | `Lock_` |
| `ConfigurationOptions` | `StackExchange.Redis` 连接配置对象（优先于 `Configuration`） | `null` |

## 依赖说明

- `YS.Knife.Lock.Core`：提供 `ILockService` 接口及扩展方法
- `YS.Knife.Core`：提供 `[Service]` / `[Options]` 自动注册能力
- `StackExchange.Redis`（2.*）

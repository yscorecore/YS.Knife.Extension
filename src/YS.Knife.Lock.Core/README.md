# YS.Knife.Lock.Core

分布式锁的抽象层，定义锁服务接口 `ILockService` 及常用扩展方法。

## 主要功能

- 定义 `ILockService` 接口：加锁、续期、解锁、查询
- 提供 `LockServiceExtentions` 扩展方法，简化常见使用场景：
  - `Lock` / `UnLock` / `Update`：以 key 本身作为锁 token 的简写形式
  - `RunOnce` / `GlobalRunOnce`：仅成功获取锁时执行一次动作
  - `RunWithLock`：获取锁后执行动作，finally 中自动解锁
  - `WaitFor` / `GlobalRunOnceOrWaitFor`：等待锁释放或“执行或等待”模式

## 接口定义

```csharp
public interface ILockService
{
    Task<bool> Lock<T>(string key, T token, TimeSpan timeSpan);
    Task<bool> Update<T>(string key, T token, TimeSpan timeSpan);
    Task<bool> UnLock<T>(string key, T token);
    Task<(bool Exists, T Token)> Query<T>(string key);
}
```

## 使用示例

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
        // 获取锁（token 默认为 key 本身），7 天内有效
        var success = await lockService.Lock("my-key", TimeSpan.FromHours(1));

        // 在锁保护下执行同步动作，结束后自动解锁
        await lockService.RunWithLock("my-key", () =>
        {
            // 临界区代码
        });

        // 全局只执行一次（永不过期的锁）
        await lockService.GlobalRunOnce("init-flag", () =>
        {
            // 初始化代码，整个集群只执行一次
        });

        // 等待其他持有者释放锁
        await lockService.WaitFor("my-key");
    }
}
```

## 依赖说明

本包仅为抽象层，无任何第三方依赖。需要配合具体实现使用：

- `YS.Knife.Lock.Impl.Memory`：基于内存缓存的单机实现
- `YS.Knife.Lock.Impl.Redis`：基于 Redis 的分布式实现

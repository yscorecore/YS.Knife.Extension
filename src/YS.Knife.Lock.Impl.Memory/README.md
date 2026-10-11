# YS.Knife.Lock.Impl.Memory

基于 `IMemoryCache` 的单机内存版 `ILockService` 实现，适用于单实例部署的应用。

## 主要功能

- 实现 `YS.Knife.Lock.Core` 定义的 `ILockService` 接口
- 基于 `IMemoryCache` 存储锁，采用双重检查锁定保证线程安全
- 支持带 token 的加锁 / 解锁 / 续期 / 查询，非 string 类型的 token 使用 JSON 序列化
- 自动注册为单例服务（`[Service(typeof(ILockService), ServiceLifetime.Singleton)]`），并通过 `IServiceRegister` 自动调用 `services.AddMemoryCache()`

## 快速开始

引用本包后无需手动注册，`MemoryLockService` 会自动注册为 `ILockService` 的实现（依赖 `YS.Knife.Core` 的服务自动注册机制）：

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
        // 加锁，token 为 key 本身，1 小时后过期
        var locked = await lockService.Lock("order-1001", TimeSpan.FromHours(1));

        // 在锁保护下执行，结束后自动解锁
        await lockService.RunWithLock("order-1001", () =>
        {
            // 临界区代码
        });

        // 续期
        await lockService.Update("order-1001", TimeSpan.FromHours(2));

        // 解锁
        await lockService.UnLock("order-1001");
    }
}
```

## 依赖说明

- `YS.Knife.Lock.Core`：提供 `ILockService` 接口及扩展方法
- `YS.Knife.Core`：提供 `[Service]` / `IServiceRegister` 自动注册能力
- `Microsoft.Extensions.Caching.Memory`（6.* ~ 9.9.0）

注意：内存锁仅在当前进程内有效，多实例部署请改用 `YS.Knife.Lock.Impl.Redis`。

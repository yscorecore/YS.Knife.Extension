# YS.Knife.Extensions.DistributedCache

`IDistributedCache` 的对象序列化扩展方法，基于 `System.Text.Json` 存取任意对象。

## 主要功能

- 读取对象：`GetObjectAsync<T>`（JSON 反序列化）
- 写入对象：`SetObjectAsync<T>`（支持滑动/相对过期时间、`DistributedCacheEntryOptions` 或无过期选项）
- 获取或添加：`GetOrAddObjectAsync<T>`，多个重载支持：
  - 值工厂 `Func<Task<T>>` 或直接值
  - 相对过期 `TimeSpan` 或绝对过期 `DateTimeOffset`
  - 从值计算过期时间并设置安全时间窗口（`safeTimeWindowSeconds`，默认 60 秒提前过期）
  - `cacheDefaultValue` 控制是否缓存空值
- `SetStringAsync` 带相对过期时间的简化重载

## 使用示例

```csharp
using Microsoft.Extensions.Caching.Distributed;

public class MyService
{
    private readonly IDistributedCache cache;

    public MyService(IDistributedCache cache) => this.cache = cache;

    public async Task<UserInfo> GetUserAsync(int id)
    {
        // 缓存 10 分钟，未命中时从数据库加载
        return await cache.GetOrAddObjectAsync(
            $"user:{id}",
            () => LoadUserFromDbAsync(id),
            TimeSpan.FromMinutes(10));
    }

    public async Task SaveTokenAsync(string key, TokenInfo token)
    {
        // 按令牌自身过期时间缓存，并提前 60 秒失效
        await cache.GetOrAddObjectAsync(
            key,
            () => Task.FromResult(token),
            value => value.ExpiresAt,
            safeTimeWindowSeconds: 60);
    }

    public async Task<T> ReadAsync<T>(string key)
        => await cache.GetObjectAsync<T>(key);
}
```

所有扩展方法定义在 `Microsoft.Extensions.Caching.Distributed` 命名空间下。

## 依赖说明

- `Microsoft.Extensions.Caching.Abstractions` (6.*)
- 项目引用 `YS.Knife.Extensions`（提供 `ToJsonText` / `AsJsonObject` 等 JSON 扩展）

目标框架：.NET 6.0。

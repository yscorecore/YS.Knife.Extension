# YS.Knife.TokenMan.Impl.Default

`ITokenManager` 的默认实现：基于分布式缓存（`IDistributedCache`）存储 Token，基于分布式锁（`ILockService`）保证并发下只有一个线程刷新 Token。

## 主要功能

- `TokenManager`：实现 `ITokenManager`
  - Token 以 JSON 序列化存储在分布式缓存中，Key 前缀 `TokenMan:`
  - 绝对过期时间 = `expiredTimeFunc` 计算结果 - 缓冲秒数（`bufferSeconds`）
  - 临近过期（剩余时间 < `preloadSeconds`）时尝试抢锁刷新；抢不到锁的请求继续使用旧 Token，避免缓存击穿
  - 首次获取抢锁失败时通过 `WaitFor` 等待持锁线程写入缓存后再读取
  - 锁最长持有 120 秒（`MaxLockSeconds`）

## 快速开始

服务通过 YS.Knife `[Service]` 特性自动注册。使用前先确保已注册：

- `IDistributedCache`（如 `AddDistributedMemoryCache()` 或 Redis 缓存）
- `ILockService`（如 `YS.Knife.Lock.Impl.Memory` 或 `YS.Knife.Lock.Impl.Redis`）

```csharp
var token = await tokenManager.GetToken<MyToken>(
    name: "wechat",
    tokenFactory: ct => FetchNewToken(ct),
    expiredTimeFunc: t => t.ExpiredTime,   // DateTimeOffset
    bufferSeconds: 120,
    preloadSeconds: 30);
```

## 依赖说明

- `YS.Knife.TokenMan.Core`：`ITokenManager` 抽象。
- `YS.Knife.Extensions.DistributedCache`：`GetObjectAsync` / `SetObjectAsync` JSON 缓存扩展。
- `YS.Knife.Lock.Core`：`ILockService` 分布式锁抽象。
- `YS.Knife.Core`（2.0.2）：`[Service]`、`[AutoConstructor]`。

# YS.Knife.KeyValue.Impl.DistributedCache

基于 `IDistributedCache` 的 `IKeyValueService` 键值存储实现。

## 主要功能

- 实现 `YS.Knife.KeyValue.Core` 中定义的 `IKeyValueService` 接口
- 通过 `IDistributedCache` 存取任意可 JSON 序列化的对象（依赖 `YS.Knife.Extensions.DistributedCache` 提供的 `GetObjectAsync` / `SetObjectAsync` 扩展）
- 支持自定义 JSON 序列化选项（`KeyValueOptions.JsonSerializerOptions`）
- 自动服务注册（基于 `YS.Knife.Core` 的 `[Service]` 特性），注入 `IDistributedCache` 后即可使用

## 快速开始

注册任意分布式缓存（内存、Redis、SqlServer 等）后，`KeyValueService` 会自动注册为 `IKeyValueService` 的实现：

```csharp
// 例如使用内存版分布式缓存
services.AddDistributedMemoryCache();

// 业务代码中注入并使用
public class MyService
{
    private readonly IKeyValueService keyValueService;

    public MyService(IKeyValueService keyValueService)
    {
        this.keyValueService = keyValueService;
    }

    public async Task Demo()
    {
        // 写入
        await keyValueService.SetValue("user:1", new { Name = "Tom", Age = 18 }, keepString: false);

        // 读取
        var user = await keyValueService.GetValue<UserInfo>("user:1");

        // 删除
        await keyValueService.Delete("user:1");
    }
}
```

自定义 JSON 序列化配置（`KeyValueOptions` 由 `[Options]` 特性自动绑定配置节）：

```csharp
services.Configure<KeyValueOptions>(options =>
{
    options.JsonSerializerOptions = new JsonSerializerOptions
    {
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
    };
});
```

## 依赖说明

- `YS.Knife.KeyValue.Core`：提供 `IKeyValueService` 接口定义
- `YS.Knife.Extensions.DistributedCache`：提供 `IDistributedCache` 的对象读写扩展方法
- `YS.Knife.Core`：提供 `[Service]` / `[AutoConstructor]` / `[Options]` 自动注册能力

运行时需要一个实际的 `IDistributedCache` 实现（如 `Microsoft.Extensions.Caching.StackExchangeRedis`）。

# YS.Knife.KeyValue.Impl.EFCore

`IKeyValueService` 的 Entity Framework Core 实现，将键值数据持久化到关系数据库。

## 主要功能

- `KeyValueService`：基于 EF Core 的键值存储实现
  - `GetValue<T>` / `SetValue` / `Delete`：读取、写入、删除键值
  - 值以字符串形式存储；`keepString = true` 且值为字符串时原样保存，否则序列化为 JSON 文本
  - 通过 `IEntityStore<KeyValueEntity<Guid>>` 访问数据库
- `KeyValueOptions`：可配置的 JSON 序列化选项（默认使用 `UnicodeRanges.All` 编码器，中文不转义）
- 通过 FlyTiger `[Service]` 特性自动注册到 DI 容器

## 快速开始

1. 在 `DbContext` 中注册实体（来自 `YS.Knife.KeyValue.Entity.EFCore`）：

```csharp
public class AppDbContext : DbContext
{
    public DbSet<KeyValueEntity<Guid>> KeyValues { get; set; }
}
```

2. 直接注入 `IKeyValueService` 使用；如需分组隔离，可注入 `IKeyValueService<TGroup>`（由 `YS.Knife.KeyValue.Default` 提供）：

```csharp
using YS.Knife.KeyValue;

public class SettingsService
{
    private readonly IKeyValueService keyValueService;

    public SettingsService(IKeyValueService keyValueService)
    {
        this.keyValueService = keyValueService;
    }

    public async Task SaveConfigAsync(CancellationToken token)
    {
        // 保存字符串（原样保存）
        await keyValueService.SetValue("site:title", "我的站点", keepString: true, token);

        // 保存对象（序列化为 JSON）
        await keyValueService.SetValue("site:config", new { Theme = "dark" }, keepString: false, token);
    }

    public async Task LoadConfigAsync(CancellationToken token)
    {
        var title = await keyValueService.GetValue<string>("site:title", token);
        var config = await keyValueService.GetValue<object>("site:config", token);
    }
}
```

## 配置说明

```csharp
services.Configure<KeyValueOptions>(opt =>
{
    opt.JsonSerializerOptions = new JsonSerializerOptions
    {
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
    };
});
```

## 依赖说明

- 目标框架：.NET 6（`net6.0`）
- `YS.Knife.KeyValue.Core`：抽象接口
- `YS.Knife.KeyValue.Default`：分组服务（一并引用以支持 `IKeyValueService<TGroup>`）
- `YS.Knife.KeyValue.Entity.EFCore`：实体定义（实现内部固定使用 `KeyValueEntity<Guid>`）
- `Microsoft.EntityFrameworkCore`、`YS.Knife.Core`、`FlyTiger`

# YS.Knife.KeyValue.Default

KeyValue 模块的默认分组服务，提供按"分组"隔离的键值存储服务。

## 主要功能

- `KeyValueGroupService<TGroup>`：`IKeyValueService<TGroup>` 的默认实现，将泛型分组 `TGroup` 注入到底层 `IKeyValueService` 中，使不同业务模块拥有相互隔离的键值命名空间
- `ServiceRegister`：自动向 DI 容器注册 `IKeyValueService<>` → `KeyValueGroupService<>`（Scoped 生命周期）

## 快速开始

1. 定义自己的分组（继承 `KeyValueGroup`，决定键名如何拼装）：

```csharp
using YS.Knife.KeyValue;

public class TenantGroup : KeyValueGroup
{
    public override string BuildUniqueKey(string key)
    {
        return $"tenant:{key}";
    }
}
```

2. 在需要的地方注入泛型分组服务：

```csharp
public class TenantConfigService
{
    private readonly IKeyValueService<TenantGroup> keyValueService;

    public TenantConfigService(IKeyValueService<TenantGroup> keyValueService)
    {
        this.keyValueService = keyValueService;
    }

    public async Task SetThemeAsync(string theme, CancellationToken token)
    {
        await keyValueService.SetValue("theme", theme, keepString: true, token);
    }

    public async Task<string> GetThemeAsync(CancellationToken token)
    {
        return await keyValueService.GetValue<string>("theme", token);
    }

    public async Task RemoveThemeAsync(CancellationToken token)
    {
        await keyValueService.Delete("theme", token);
    }
}
```

## 依赖说明

- 目标框架：.NET 6（`net6.0`）
- `YS.Knife.KeyValue.Core`：抽象接口 `IKeyValueService` / `IKeyValueService<T>` / `KeyValueGroup`
- `YS.Knife.Core`、`FlyTiger`：服务自动注册与构造器生成
- 实际的持久化由底层 `IKeyValueService` 实现提供（如 `YS.Knife.KeyValue.Impl.EFCore`）

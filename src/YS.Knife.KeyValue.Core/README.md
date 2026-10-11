# YS.Knife.KeyValue.Core

键值存储功能的抽象层，定义核心接口 `IKeyValueService`、分组机制 `KeyValueGroup` 及便捷扩展方法，不包含具体存储实现。

## 主要功能

### 核心接口

```csharp
public interface IKeyValueService
{
    Task<T> GetValue<T>(string key, CancellationToken cancellationToken = default);
    Task SetValue(string key, object value, bool keepString, CancellationToken cancellationToken = default);
    Task Delete(string key, CancellationToken cancellationToken = default);
}

public interface IKeyValueService<T> : IKeyValueService where T : IKeyValueGroup
{
}
```

- `Get(key)` 为 `GetValue<object>(key)` 的默认实现
- `keepString` 参数指示字符串值是否按原样保存（不做 JSON 序列化）

### 分组机制

```csharp
public abstract class KeyValueGroup : IKeyValueGroup
{
    public abstract string BuildUniqueKey(string key);
}
```

通过继承 `KeyValueGroup` 定义逻辑分组，为键添加前缀，实现不同业务域的键隔离：

```csharp
public class SystemSettings : KeyValueGroup
{
    public override string BuildUniqueKey(string key) => $"sys:{key}";
}
```

### 扩展方法（`KeyValueServiceExtensions`）

在 `IKeyValueService` 上提供带分组的重载：

```csharp
await service.SetValue(group, "theme", "dark");           // 默认 keepString=false
await service.SetValue(group, "theme", "dark", true);     // 保持字符串
var value = await service.GetValue<string>(group, "theme");
await service.Delete(group, "theme");
```

分组为 `null` 时直接使用原始 key。

## 使用方式

在业务代码中面向 `IKeyValueService` 编程，存储实现由以下包提供：

- `YS.Knife.KeyValue.Impl.EFCore`：基于 Entity Framework Core 的数据库存储
- `YS.Knife.KeyValue.Impl.DistributedCache`：基于分布式缓存的存储
- `YS.Knife.KeyValue.Api.AspnetCore`：HTTP API 层

## 依赖

- `YS.Knife.Extensions`（项目引用）
- 无第三方运行时依赖

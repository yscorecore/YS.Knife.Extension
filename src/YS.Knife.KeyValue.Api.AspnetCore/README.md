# YS.Knife.KeyValue.Api.AspnetCore

键值存储的 ASP.NET Core Web API 层，为每个具体的 `KeyValueGroup` 分组自动生成一个泛型 Controller，提供键值的增、删、查 HTTP 接口。

## 主要功能

- 泛型控制器 `KeyValueController<T>`（`T : KeyValueGroup`），路由 `api/[controller]`：

| 路由 | 方法 | 说明 |
| --- | --- | --- |
| `GET api/{group}/{key}` | `GetValue` | 获取指定键的值 |
| `POST api/{group}` | `SetValue` | 设置键值（Body 为 `{"key": ..., "value": ...}`），查询参数 `keepstring` 控制是否保持字符串原样（默认 `true`） |
| `DELETE api/{group}/{key}` | `Delete` | 删除指定键 |

- `KeyValueGroupGenericControllerAttribute` 继承自 `GenericControllerAttribute`，启动时扫描所有非 System/Microsoft 程序集中**继承 `KeyValueGroup` 且带 `[Service]` 特性的非抽象类**，为每个分组生成独立 Controller，Controller 名即分组类名。

## 快速开始

1. 定义一个分组并注册 `IKeyValueService` 实现（如 `YS.Knife.KeyValue.Impl.EFCore` 或 `YS.Knife.KeyValue.Impl.DistributedCache`）：

```csharp
[Service]
public class SystemSettings : KeyValueGroup
{
    public override string BuildUniqueKey(string key) => $"sys:{key}";
}
```

2. 引用本包后启动应用，即自动获得：

```
GET    /api/SystemSettings/{key}
POST   /api/SystemSettings?keepstring=true
DELETE /api/SystemSettings/{key}
```

## 依赖

- `YS.Knife.KeyValue.Core`（项目引用）：`KeyValueGroup`、`IKeyValueService` 抽象
- `YS.Knife.AspnetCore`（项目引用）：泛型 Controller 支持（`GenericControllerAttribute`）
- 目标框架 `Microsoft.NET.Sdk.Web`，面向 ASP.NET Core

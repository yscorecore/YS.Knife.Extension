# YS.Knife.ServiceSwitch

基于配置的服务开关扩展，支持在运行时通过名称或配置项在多个同类型服务实现之间切换。

## 主要功能

为 `IServiceProvider` 提供三个扩展方法（命名空间为 `System`，引用包后无需额外 using）：

- `GetServiceByName<T>(name)`：按名称从 `IDictionary<string, T>` 中获取服务实例；名称为空时返回默认注册的服务
- `GetServiceByNameOrConfiguationSwitch<T>(name)`：优先使用传入名称，否则依次读取配置 `ServiceSwitch:{T.Name}`、`ServiceSwitch:{T.FullName}` 确定服务名称
- `GetServiceByConfiguationSwitch<T>()`：完全通过配置决定使用哪个命名服务实现

## 使用示例

### 1. 按名称解析命名服务

```csharp
// 前提：容器中注册了 IDictionary<string, ISmsService>（各实现以名称为键）
var sms = serviceProvider.GetServiceByName<ISmsService>("submail");
await sms.SendSms(smsInfo);
```

### 2. 通过配置切换实现

`appsettings.json`：

```json
{
  "ServiceSwitch": {
    "ISmsService": "submail"
  }
}
```

代码中：

```csharp
// 读取配置 ServiceSwitch:ISmsService 的值作为服务名进行解析
var sms = serviceProvider.GetServiceByConfiguationSwitch<ISmsService>();
```

也可以传入名称兜底（传入的名称优先于配置）：

```csharp
var sms = serviceProvider.GetServiceByNameOrConfiguationSwitch<ISmsService>("submail");
```

## 规则说明

- 服务名解析顺序：显式传入的 `name` → `ServiceSwitch:{T.Name}` → `ServiceSwitch:{T.FullName}`
- 最终名称为空时，直接从容器解析默认的 `T`（`GetRequiredService<T>()`）
- 名称非空但在 `IDictionary<string, T>` 中找不到时抛出异常

## 依赖说明

- `Microsoft.Extensions.Configuration.Abstractions`（6.* 及以上，< 99.99）
- `Microsoft.Extensions.DependencyInjection.Abstractions`（6.* 及以上，< 99.99）

无传递性框架依赖，可用于任意使用 `Microsoft.Extensions.DependencyInjection` 的应用。

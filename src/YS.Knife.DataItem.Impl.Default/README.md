# YS.Knife.DataItem.Impl.Default

数据项（DataItem）服务的默认实现：通过反射调用标注了 `DataItem` 的服务方法，并支持在 Web 环境下直接从 QueryString 绑定参数加载数据。

## 主要功能

- `DataItemService`：实现 `IDataItemService`：
  - `GetItem(name, args, cancellationToken)`：按名称查找数据项条目（`DataItemEntry`），从 DI 容器解析对应服务实例并反射调用方法，自动适配 `Task<T>` / `ValueTask<T>` 返回值，以及可选的 `CancellationToken` 参数。
  - `GetAllDataItems()`：列出全部可用数据项（`DataItemDesc`）。
  - `GetEntry(name)`：获取数据项条目元数据（方法、参数、返回类型等）。
- `DataItemWebService`：实现 `IDataItemWebService`，面向 Web 场景：
  - `ListItems()`：列出全部数据项。
  - `LoadData(di[], cancellationToken)`：批量加载多个数据项；每个数据项的参数通过 MVC 的 ModelBinding 机制从 QueryString 绑定。
- `DataItemValueQueryStringProvider`：自定义 QueryString 值提供者，同时支持 `itemName.key` 命名参数与通用 `key` 参数，实现多数据项参数隔离与共享。
- 内部 `ServiceRegister` 自动注册 `IActionContextAccessor`。

## 使用示例

```csharp
// 定义数据项（DataItemAttribute 来自 YS.Knife.DataItem.Core）
public class CommonDataItems
{
    [DataItem("provinceList")]
    public Task<List<Province>> GetProvinces(string countryCode, CancellationToken cancellationToken)
        => ...;
}

// Web 请求：GET /api/dataitem?di=provinceList&provinceList.countryCode=CN
// DataItemWebService 会从 QueryString 绑定参数并调用：
public class DataItemController : ControllerBase
{
    private readonly IDataItemWebService webService;

    [HttpGet]
    public Task<Dictionary<string, object>> Load([FromQuery] string[] di, CancellationToken ct)
        => webService.LoadData(di, ct);
}
```

## 依赖说明

- `YS.Knife.DataItem.Core`：提供 `IDataItemService`、`DataItemEntry`、`DataItemAttribute`、`AssemblyDataItemEntryFinder` 等抽象与扫描机制。
- `YS.Knife.Extensions`、`YS.Knife.Core`（`[Service]`、`[AutoConstructor]`、`[Logger]`）。
- `Microsoft.AspNetCore.Mvc.Core`（2.2.0 ~ 9.0.0）：Web 参数绑定能力。

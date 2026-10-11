# YS.Knife.DataItem.Core

数据项（DataItem）模块的抽象层。数据项是一种约定：在接口方法上标注 `[DataItem]` 特性，即可将方法声明为可动态调用的"数据项"，通常用于为前端或报表系统提供统一的数据加载入口。

## 主要功能

- `DataItemAttribute`：标注在接口方法上，声明数据项名称及是否自动注册元数据
- `IDataItemService`：按名称动态调用数据项方法，并查询全部数据项描述
- `IDataItemWebService`：面向 Web 的批量数据加载接口
- `AssemblyDataItemEntryFinder`：扫描当前 AppDomain 中的程序集，查找所有标注了 `[DataItem]` 的接口方法并构建数据项入口字典（`DataItemEntry`）

## 快速开始

1. 在服务接口上声明数据项（方法签名要求为 `Task<T>` 或 `ValueTask<T>`）：

```csharp
using YS.Knife.DataItem;

public interface IReportService
{
    [DataItem("user-count")]
    Task<int> GetUserCountAsync(CancellationToken cancellationToken);

    [DataItem("orders")]
    Task<List<Order>> GetOrdersAsync(DateTime from, DateTime to, CancellationToken cancellationToken);
}
```

2. 注入 `IDataItemService` 动态调用（实现由 `YS.Knife.DataItem.Impl.Default` 提供）：

```csharp
public class DashboardService
{
    private readonly IDataItemService dataItemService;

    public DashboardService(IDataItemService dataItemService)
    {
        this.dataItemService = dataItemService;
    }

    public async Task LoadAsync(CancellationToken cancellationToken)
    {
        // 按名称调用数据项，args 为方法的参数（不含 CancellationToken）
        var count = await dataItemService.GetItem("user-count", Array.Empty<object>(), cancellationToken);

        // 查询所有数据项的名称与描述
        var items = await dataItemService.GetAllDataItems();
    }
}
```

## 数据项入口扫描

`AssemblyDataItemEntryFinder`（单例）在首次访问 `All` 属性时扫描 AppDomain 中非 `System`/`Microsoft` 前缀的程序集，把所有接口上标注 `[DataItem]` 的方法整理为 `DataItemEntry`（包含名称、服务类型、方法信息、返回类型、参数列表等元数据）。

## 依赖说明

- 目标框架：.NET 6（`net6.0`）
- `FlyTiger`（单例模式、代码生成）
- 实现层请参考 `YS.Knife.DataItem.Impl.Default`；Web API 层请参考 `YS.Knife.DataItem.Api.AspnetCore`

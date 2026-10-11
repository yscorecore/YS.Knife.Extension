# YS.Knife.DataItem.Api.AspnetCore

数据字典（DataItem）Web API 层，自动为所有标注 `[DataItem]` 特性的数据字典方法生成 HTTP 查询接口，无需手写 Controller。

## 工作原理

- 基于 `YS.Knife.AspnetCore` 的泛型控制器机制：`DataItemController<...>` 是一个带 10 个泛型参数（返回值类型 + 最多 8 个参数类型 + 名称标记类型）的开放泛型控制器。
- `DataItemGenericControllerAttribute` 扫描 `AssemblyDataItemEntryFinder` 找到的所有数据字典条目，为每个条目构造一个封闭泛型控制器类型并注册到 MVC。
- 路由格式为 `api/DataItem/{数据字典名称}`，每个条目的参数作为 Query 参数暴露，参数名取自方法参数名。
- 内部通过 `DynamicType`（Reflection.Emit）动态生成名称标记类型，用 `NullObject` 填充未使用的参数位。

## 使用示例

假设在业务代码中定义了数据字典（`YS.Knife.DataItem.Core`）：

```csharp
public class CommonDataItems
{
    [DataItem("citys")]
    public List<KeyValuePair<string, string>> GetCitys(string province)
    {
        ...
    }
}
```

应用启动时注册泛型控制器特性提供程序：

```csharp
builder.Services.AddControllers()
    .ConfigureApplicationPartManager(m =>
        m.FeatureProviders.Add(new GenericControllerFeatureProvider()));
```

随后即可通过 HTTP 访问：

```
GET /api/DataItem/citys?province=浙江
```

Controller 内部调用 `IDataItemService.GetItem(name, args, cancellationToken)` 获取数据并返回。

## 说明

- 数据字典的定义与服务抽象位于 `YS.Knife.DataItem.Core`；本项目是其 Web API 暴露层（命名规则：`*.Api.AspnetCore` 为 API 层）。
- 单个数据字典最多支持 8 个参数，超出会抛出 `InvalidOperationException`。

## 依赖

- 项目引用：`YS.Knife.AspnetCore`、`YS.Knife.DataItem.Core`、`YS.Knife.Extensions`
- 目标框架：.NET 6+（`Microsoft.NET.Sdk.Web`）

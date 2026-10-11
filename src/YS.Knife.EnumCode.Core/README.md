# YS.Knife.EnumCode.Core

枚举码（EnumCode）能力的抽象层：定义枚举码服务与加载器接口，并提供从程序集枚举类型中提取码表信息的工具。

## 主要功能

- `IEnumCodeService`：枚举码服务接口，`GetAllEnumCode()` 返回按枚举类型分组的全部码表（带 `[Operation("all", "获取所有的枚举值")]` 元数据）。
  - 内嵌 `CodeInfo` 记录：`Key`（数值）、`Name`（字段名）、`Display`（显示名）、`Description`、`Group`、`Order`（不参与序列化）。
- `ICodeLoader`：码表加载器接口，定义 `Priority` 与 `AllCodes()`；系统中可注册多个加载器（如程序集加载、数据库加载），按优先级合并。
- `AssemblyEnumCodes`：单例工具类，`LoadAssemblyEnumCodes(string[] assemblies)` 加载指定程序集，扫描其中所有公共枚举类型，结合 `DisplayAttribute` / `DescriptionAttribute` 提取显示名、描述、分组与排序信息，生成 `CodeInfo` 列表。

## 使用示例

```csharp
public enum OrderStatus
{
    [Display(Name = "待支付", Order = 1)]
    Pending = 0,
    [Display(Name = "已支付", Order = 2, Description = "支付完成")]
    Paid = 1,
}

// 从程序集提取码表
var dic = AssemblyEnumCodes.Instance.LoadAssemblyEnumCodes(new[] { "MyApp.Domain" });
// dic["OrderStatus"] => [ { Key=0, Name="Pending", Display="待支付", Order=1 }, ... ]

// 实现自定义加载器（例如从缓存或数据库）
public class MyCodeLoader : ICodeLoader
{
    public int Priority => 100;

    public Task<Dictionary<string, List<IEnumCodeService.CodeInfo>>> AllCodes()
        => Task.FromResult(AssemblyEnumCodes.Instance.LoadAssemblyEnumCodes(new[] { "MyApp.Domain" }));
}
```

## 依赖说明

- `YS.Knife.Operations.Core`：提供 `[Operation]` 元数据。
- 实现层可参考 `YS.Knife.EnumCode.Loader.Impl.EFCore`（从数据库加载并同步枚举码）。

# YS.Knife.Metadata.Impl.Mvc

`IMetadataService` 的 MVC 实现：基于 ASP.NET Core MVC 的 `IModelMetadataProvider`，从标注了 `[Metadata]` 的实体类型自动生成结构化元数据。

## 主要功能

- `MetadataService`：实现 `IMetadataService`
  - `ListAll()`：返回所有已注册元数据名称
  - `GetMetadataInfo(name)`：从类型反射生成 `MetadataInfo`
- 自动解析内容：
  - 列信息：属性路径（遵循 `JsonPropertyName` / JSON 命名策略）、`Display` / `DisplayName` / `Description` 特性、显示格式、排序
  - 数据类型推断：基元类型、可空类型、数组与 `IEnumerable<T>` 识别为 `IsArray`；复杂对象递归展开为点路径（`a.b.c`）
  - 编辑器数据源：枚举属性自动附加 `EditorEnumSource`，显式标注的 `EditorSourceAttribute` 优先
  - 查询过滤器：读取 `[QueryFilter]` 特性
- 拦截器管线：全局拦截器（`IMetadataInterceptor.IsGlobal`）与类上 `[MetadataInterceptor("name")]` 指定的拦截器按 `Priority` 依次执行，可动态改写元数据
- `ServiceRegister`：启动时自动扫描程序集中所有带 `[Metadata]` 特性的类型并注册到 `MetadataOptions`

## 快速开始

```csharp
[Metadata("product")]
public class ProductDto
{
    [Display(Name = "名称")]
    [QueryFilter(Operators.Contains)]
    public string Name { get; set; }

    [Display(Name = "价格")]
    [QueryFilter(Operators.Between)]
    public decimal Price { get; set; }
}
```

服务随 YS.Knife 自动注册（`[Service]` 特性 + `ServiceRegister` 扫描），直接注入使用：

```csharp
public class MetadataController : ControllerBase
{
    private readonly IMetadataService metadataService;

    [HttpGet("{name}")]
    public Task<MetadataInfo> Get(string name)
        => metadataService.GetMetadataInfo(name);
}
```

自定义拦截器：

```csharp
[Service] // 注册为命名拦截器，Name 由注册键决定
public class TenantColumnInterceptor : IMetadataInterceptor
{
    public int Priority => 0;
    public string Name => "tenant";

    public Task Process(MetadataInterceptorContext context, CancellationToken cancellationToken)
    {
        // 动态增删/修改列
        return Task.CompletedTask;
    }
}
```

## 依赖说明

- `YS.Knife.Metadata.Core`：抽象与模型。
- `YS.Knife.Core`（2.0.2）：`[Service]`、`[AutoConstructor]`、`IServiceRegister` 等。
- 依赖 ASP.NET Core MVC（`IModelMetadataProvider`、Http.Json 的 `JsonOptions`），需在 Web 项目中使用。

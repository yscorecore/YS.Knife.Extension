# YS.Knife.Metadata.Core

实体元数据（Metadata）抽象层。定义了用于向前端/低代码场景描述实体结构（列、显示名、数据源、查询过滤器等）的核心模型与接口。

## 主要功能

- `IMetadataService`：元数据查询接口
  - `ListAll()`：列出所有已注册的元数据名称
  - `GetMetadataInfo(name)`：获取指定名称的元数据详情
- `MetadataInfo` / `MetadataClolumnInfo`：元数据描述模型，包含显示名、描述、列路径、数据类型、显示格式、排序、数据源（`DataSourceInfo`）、查询过滤器（`QueryFilterInfo`）、标签（`Tags`）与输入提示（`Prompt`）
- 特性标注：
  - `[Metadata("name")]`：标注类为元数据实体（可多个）
  - `[MetadataInterceptor("interceptorName")]`：为类指定命名的元数据拦截器
  - `[QueryFilter(Operators.Equal)]`：标注属性为查询过滤字段，可指定操作符与默认值
  - 编辑器数据源：`[EditorDataSource]`、`[EditorEnumSource]`、`[EditorConstantSource]`、`[EditorNamedConstantSource]`
  - `[BindToSource("refName")]`：声明列的绑定数据源，输出到列的 `BindTo` 字段；与编辑器数据源并行，可同时标注
  - `[Tag("key", "value")]`：为类或属性添加自定义标签，可叠加多个；同名 key 重复时抛异常
- `Operators`：内置查询操作符常量（`==`、`!=`、`between`、`in`、`contains` 等）
- `IMetadataInterceptor`：元数据拦截器接口，支持全局或按名称拦截并修改生成的 `MetadataInfo`，按 `Priority` 排序执行
- `MetadataOptions`：元数据注册表，提供 `AddMeta` / `AddOrReplaceMeta` 等注册方法

## 快速开始

定义元数据实体：

```csharp
using System.ComponentModel.DataAnnotations;
using YS.Knife.Metadata;

[Metadata("user")]
[MetadataInterceptor("tenantFilter")]
[Tag("category", "user")]
public class UserDto
{
    [Display(Name = "用户名")]
    [Tag("sensitive", "true")]
    public string Name { get; set; }

    [Display(Name = "状态")]
    [EditorEnumSource("UserStatus")]
    public int Status { get; set; }

    [Display(Name = "创建时间")]
    [QueryFilter(Operators.Between)]
    public DateTime CreateTime { get; set; }
}
```

消费方（由实现层提供，如 `YS.Knife.Metadata.Impl.Mvc`）：

```csharp
var names = await metadataService.ListAll();
var info = await metadataService.GetMetadataInfo("user");
foreach (var col in info.Columns)
{
    Console.WriteLine($"{col.PropertyPath} {col.DisplayName} {col.DataTypeName}");
}

// info.Tags => { "category" => "user" }
// col.Tags  => { "sensitive" => "true" }（仅配置了 [Tag] 的属性才有值）
```

## 依赖说明

本包无任何外部依赖，仅定义抽象与模型。具体实现见 `YS.Knife.Metadata.Impl.Mvc`（基于 ASP.NET Core MVC 的 `IModelMetadataProvider` 生成元数据）。

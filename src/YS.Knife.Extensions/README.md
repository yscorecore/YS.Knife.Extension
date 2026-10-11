# YS.Knife.Extensions

YS.Knife 系列的基础扩展方法库，提供字符串、集合、表达式、IQueryable、枚举、类型等常用工具方法。

## 主要功能

### 字符串与 JSON（StringExtensions）

- `ToJsonText` / `AsJsonObject<T>` / `AsJsonElement`：基于 `System.Text.Json` 的序列化/反序列化
- `JoinUrl` / `CombinQueryString`：URL 拼接
- `WithStyle(NameStyle)`：命名风格转换；`Truncate`：截断；`IsValidBase64Strict`：严格 Base64 校验

### 集合（EnumerableExtenstions / CollectionExtensions / DictionaryExtensions）

- `AsList` / `AsArray`：单元素转集合；`RandomOne`：随机取一项
- `OrderBySequence`：按给定序列排序
- `HasDuplicate`：重复项检测；`ConcatItems`：拼接元素
- `AppendTo` / `MergeTo` / `UpdateTo` / `SaveTo`：按键将源集合差异同步到目标集合（配合 `CollectionSaveMode`）
- `DictionaryExtensions.Merge`：合并字典

### IQueryable 与表达式

- `WhereIf`：条件 Where；`WhereItemsAnd` / `WhereItemsOr`：按元素集合组合筛选
- `Summary`：分组汇总；`Map<R>`：非泛型转泛型
- `TryOrderByEntityKey`：尝试按实体主键排序
- `ExpressionExtensions`：`ReplaceFirstParam`、`CombinAndAlso`、`CombinOrElse` 表达式组合

### 其他

- `EnumExtensions.GetDisplayName`：获取枚举 `Display` 特性名称
- `ComparableExtensions.IsBetween` / `IsNotBetween`：区间判断
- `ObjectExtensions.CopyTo`：同名属性拷贝
- `TypeExtensions.GetDefaultValue`：获取类型默认值
- `ContentTypeMappings`：文件名/扩展名与 ContentType 互查
- `StreamBody`：文件/流/字节数组包装（含 `FromLocalFile`、`FromStream`、`FromBytes`），用于 HTTP 上传等场景

## 使用示例

```csharp
// JSON
var json = dto.ToJsonText();
var obj = json.AsJsonObject<MyDto>();

// URL
var url = "https://api.example.com/".JoinUrl("users", "1");

// 条件查询
var query = dbContext.Users
    .WhereIf(!string.IsNullOrEmpty(name), p => p.Name.Contains(name))
    .TryOrderByEntityKey();

// 表达式组合
var combined = predicates.CombinOrElse(typeof(User));

// 集合差异同步
source.SaveTo(target, s => s.Id, t => t.Id, (s, t) => s.CopyTo(t));

// 枚举显示名
string text = MyEnum.Value1.GetDisplayName();

// ContentType
string mime = ContentTypeMappings.GetContentTypeByFileName("a.png"); // image/png
```

## 依赖说明

- 无第三方包依赖，仅使用 .NET 内置 API

目标框架：.NET 6.0。

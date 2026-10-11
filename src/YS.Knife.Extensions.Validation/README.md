# YS.Knife.Extensions.Validation

`System.ComponentModel.DataAnnotations` 的扩展验证特性库，补充常用业务校验规则。

## 主要功能

### 属性间比较

- `GreatThanAttribute(otherPropertyName)`：当前属性必须大于（可 `AllowEquals` 允许等于）另一属性，错误消息支持对方属性的 `Display` 名称
- `LessThanAttribute(otherPropertyName)`：必须小于另一属性

### 时间验证

- `FutureDateAttribute`：只能是将来的日期（`AllowToday` 控制是否含今天，默认 true）
- `PastDateAttribute`：只能是过去的日期
- `FutureTimeAttribute`：只能是将来的时间点

### 集合/枚举值验证

- `InAttribute(params object[] items)`：值必须在指定项中
- `NotInAttribute(params object[] items)`：值不允许在指定项中
- `ValidJsonAttribute`：字符串必须是合法 JSON

### 集合元素验证

- `ItemRequiredAttribute`：集合中每个元素均不可为空
- `ItemInAttribute`：集合中每个元素必须在指定项中
- `ItemUniqueAttribute`：集合元素不可重复

### 字符串规则

- `NameRuleAttribute`：可配置的命名规则（大小写字母、Unicode 字母、数字、下划线、连字符、空格、最小/最大长度）
- `StartsWithAttribute`：字符串必须以指定前缀开头

## 使用示例

```csharp
using System.ComponentModel.DataAnnotations;

public class CreateOrderDto
{
    [FutureDate(AllowToday = false)]
    public DateTime DeliveryDate { get; set; }

    [GreatThan(nameof(MinPrice), AllowEquals = true)]
    public decimal MaxPrice { get; set; }
    public decimal MinPrice { get; set; }

    [In("A", "B", "C", ErrorMessage = "无效的等级")]
    public string Level { get; set; }

    [ItemRequired]
    [ItemUnique]
    public List<string> Tags { get; set; }

    [NameRule(AllowDigits = true, AllowUnderscore = true, MinLength = 2, MaxLength = 32)]
    public string UserName { get; set; }

    [ValidJson]
    public string ExtraData { get; set; }
}
```

所有特性定义在 `System.ComponentModel.DataAnnotations` 命名空间下，可直接与 ASP.NET Core 模型验证配合使用。

## 依赖说明

- 无第三方依赖，基于 .NET 内置的 DataAnnotations

目标框架：.NET 6.0。

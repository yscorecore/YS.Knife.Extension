# YS.Knife.Extensions.Json

`System.Text.Json` 的序列化特性与转换器扩展库，提供枚举、日期格式化、字段脱敏、单向忽略等能力。

## 主要功能

- 枚举序列化：`CamelCaseJsonStringEnumConverter`（枚举名转 camelCase 字符串）
- 日期格式化：
  - `JsonDateTimeConverter` / `JsonDateTimeOffsetConverter`（按指定格式读写）
  - `JsonDateTimeFormatAttribute`（属性级指定格式，自动匹配 `DateTime` / `DateTimeOffset`）
- 序列化方向控制：
  - `JsonIgnoreWhenReadAttribute`：反序列化时忽略（读取后赋默认值），序列化正常输出
  - `JsonIgnoreWhenWriteAttribute`：序列化时输出默认值，反序列化正常读取
- 字符串脱敏（仅序列化输出时生效）：
  - `JsonMaskAttribute`：正则替换或整体替换为掩码
  - `JsonMaskCharAttribute`：按起始索引（支持负数从尾部计）与长度替换为掩码字符
  - `JsonNameMaskAttribute`：姓名脱敏（如 `张*三`）
  - `JsonTelMaskAttribute`：手机号脱敏（保留前 3 后 4，中间 4 位掩码）

## 使用示例

```csharp
using System.Text.Json.Serialization;

public class UserDto
{
    // 枚举输出为 camelCase 字符串
    [JsonConverter(typeof(CamelCaseJsonStringEnumConverter))]
    public UserStatus Status { get; set; }

    // 日期按指定格式序列化
    [JsonDateTimeFormat("yyyy-MM-dd HH:mm:ss")]
    public DateTime CreateTime { get; set; }

    // 姓名脱敏：张三 -> 张*
    [JsonNameMask]
    public string Name { get; set; }

    // 手机号脱敏：138****1234
    [JsonTelMask]
    public string Phone { get; set; }

    // 自定义正则掩码
    [JsonMask(@"(\w{3}).*(@.*)", "$1***$2")]
    public string Email { get; set; }

    // 客户端提交时忽略该字段，但响应中正常输出
    [JsonIgnoreWhenRead]
    public string ServerOnlyField { get; set; }

    // 响应中不输出真实值，但可正常接收客户端提交
    [JsonIgnoreWhenWrite]
    public string Password { get; set; }
}
```

所有特性与转换器定义在 `System.Text.Json.Serialization` 命名空间下，无第三方依赖。

## 依赖说明

- 仅依赖 `System.Text.Json`（.NET 内置）

目标框架：.NET 6.0。

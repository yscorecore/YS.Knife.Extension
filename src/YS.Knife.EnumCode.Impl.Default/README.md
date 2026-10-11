# YS.Knife.EnumCode.Impl.Default

`IEnumCodeService` 的默认实现：聚合多个代码加载器（`ICodeLoader`）的结果，并内置从程序集枚举类型反射加载码表的加载器。

## 主要功能

- `EnumCodeService`：实现 `IEnumCodeService.GetAllEnumCode()`
  - 按 `ICodeLoader.Priority` 顺序聚合所有加载器返回的码表
  - 同名分类按 `CodeInfo.Key` 合并（后加载者覆盖先加载者），并按 `Order` 排序
  - 分类名按配置的命名风格（`EnumCodeOptions.NameStyle`）统一转换
- `AssemblyEnumCodeLoader`：内置加载器（Priority = 0，单例 + 懒加载缓存）
  - 从配置的程序集中反射枚举类型，读取 `Display` / `Description` 等特性生成 `CodeInfo`
- `EnumCodeOptions`：配置项
  - `Assemblies`（必填）：包含枚举类型的程序集名称列表
  - `NameStyle`：分类名命名风格

## 快速开始

1. 配置（`appsettings.json` 中 `EnumCode` 节）：

```json
{
  "EnumCode": {
    "NameStyle": "Camel",
    "Assemblies": [ "MyApp.Domain" ]
  }
}
```

2. 枚举定义（位于配置的程序集中）：

```csharp
public enum OrderStatus
{
    [Display(Name = "待支付", Order = 1)]
    Pending = 1,
    [Display(Name = "已支付", Order = 2)]
    Paid = 2
}
```

3. 使用（服务通过 YS.Knife `[Service]` 自动注册）：

```csharp
var all = await enumCodeService.GetAllEnumCode();
// all["orderStatus"] => [ { Key=1, Name="Pending", Display="待支付" }, ... ]
```

如需补充数据库等其他来源的码表，实现额外的 `ICodeLoader`（设置更高的 `Priority`）即可自动合并。

## 依赖说明

- `YS.Knife.EnumCode.Core`：`IEnumCodeService`、`ICodeLoader`、`AssemblyEnumCodes`、`CodeInfo`。
- `YS.Knife.Extensions`：命名风格转换等工具。
- `YS.Knife.Core`（2.0.2）：`[Service]`、`[AutoConstructor]`、`[Options]`。

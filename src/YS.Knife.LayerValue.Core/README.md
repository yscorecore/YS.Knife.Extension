# YS.Knife.LayerValue.Core

分层值（LayerValue）能力的抽象层：定义按“设置组 + 键 + 角色码”存储与读取分层值的接口、模型及便捷的转换扩展。

## 主要功能

- `LayerValueInfo` / `LayerValueInfo<T>`：分层值记录，由 `Key`（设置键）与 `RoleCode`（角色码）联合标识，`Value` 为 JSON 字符串或强类型值。
- `GroupValueInfo` / `GroupValueInfo<T>`：按 `Key` 分组后的值信息，包含该键下所有角色的分层值及首个生效值。
- `ILayerService`：分层值读取接口：
  - `GetLayerValuesByKeys(group, keys)`、`GetLayerValues(group, keys, roleCodes)`、`GetLayerValuesByRoleCodes(group, roleCodes)`。
  - 静态字段 `JsonOptions` 提供统一的大小写不敏感反序列化配置。
- `ILayerValueAssignService`：分层值赋值接口：
  - `AssignByRole(LayerValueAssignByRoleInfo)`：按角色批量赋值（`Group` + `RoleCode` + `KeyValues`）。
  - `AssignByKey(LayerValueAssginByKeyInfo)`：按键批量赋值（`Group` + `Key` + `RoleValues`）。
  - `GetLayerValueByRole<T>` / `GetLayerValueByKey<T>`：按角色或按键读取整组值。
- 扩展方法（`LayerServiceExtensions` / `LayerValueInfoExtensions`）：
  - `GetGroupValues(...)`、`ToGroupValues()`：将分层值按键分组。
  - `As<T>()`、`ToLayerValueInfo<T>()`：将 JSON 字符串值转换为强类型。

## 使用示例

```csharp
public class ThemeService
{
    private readonly ILayerService layerService;

    public async Task<ThemeConfig?> GetThemeAsync(string[] roleCodes)
    {
        // 读取 ui 组中 theme 键在所有角色下的分层值，并取强类型
        var values = await layerService
            .GetLayerValues("ui", new[] { "theme" }, roleCodes)
            .As<ThemeConfig>();
        return values.FirstOrDefault()?.Value;
    }
}

// 赋值
public class SettingAdminService
{
    private readonly ILayerValueAssignService assignService;

    public Task SetThemeForRoleAsync(string roleCode, ThemeConfig config)
        => assignService.AssignByRole(new ILayerValueAssignService.LayerValueAssignByRoleInfo
        {
            Group = "ui",
            RoleCode = roleCode,
            KeyValues = new() { ["theme"] = config }
        });
}
```

## 依赖说明

- `YS.Knife.Extensions`：提供 `AsJsonObject<T>` 等 JSON 扩展。
- 具体存储实现由对应的 `Impl` 包提供（如基于 EF Core 的分层值存储）。

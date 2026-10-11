# YS.Knife.LayerSetting.Impl.EFCore

基于 Entity Framework Core 的分层设置（LayerSetting）实现：负责设置定义的读取/管理，以及按当前用户逻辑角色计算最终生效的分层设置值。

## 主要功能

- `LayerSettingService`：实现 `ILayerSettingService`：
  - `GetLayerSetting(group)` / `GetLayerSettingObject<T>(group)`：先读取设置组配置的 `RoleProviders`，过滤当前用户的逻辑角色码，再委托 `ILayerService` 按角色码计算该设置组的最终生效值（字典或强类型对象）。
- `LayerSettingManagerService`：实现 `ILayerSettingManagerService`：
  - `QuerySettings(LimitQueryInfo)`：分页查询设置定义（`PagedList<SettingInfo>`）。
  - `LoadFromFile(StreamBody)`：从文件加载设置定义（`FileSettingInfo`）。
  - `RefreshSettings()`：扫描 `LayerSettingOptions.Assemblies` 配置的程序集，通过 `FindLayerSettings()` 发现设置定义并以 Merge 模式保存到数据库。
  - `SaveSetting(setting, saveMode)` / `RemoveSetting(group)`：保存（支持集合增量保存模式 `CollectionSaveMode`）或删除设置组及其属性。
- `LayerSettingOptions`：配置项，`Assemblies` 指定需要扫描设置定义的程序集。

## 使用示例

```json
{
  "LayerSettingOptions": {
    "Assemblies": [ "MyApp.Settings" ]
  }
}
```

```csharp
public class MailConfigService
{
    private readonly ILayerSettingService layerSettingService;

    // 按当前用户角色获取 "mail" 设置组的强类型配置
    public Task<MailSettings> GetMailSettingsAsync()
        => layerSettingService.GetLayerSettingObject<MailSettings>("mail");
}

// 管理端：启动时同步设置定义
public class SettingInitializer
{
    private readonly ILayerSettingManagerService managerService;

    public Task InitAsync(CancellationToken ct) => managerService.RefreshSettings(ct);
}
```

## 依赖说明

- `YS.Knife.LayerSetting.Core`：提供 `ILayerSettingService`、`ILayerSettingManagerService`、`SettingInfo` 等抽象。
- `YS.Knife.LayerSetting.Entity.EFCore`：提供 `SettingEntity` / `SettingPropertyEntity` 实体。
- `YS.Knife.LogicRoles.Core`：提供 `ILogicRoleService` 逻辑角色服务。
- `YS.Knife.Core`：`[Service]`、`[AutoConstructor]`、`[Options]`、`[Mapper]` 源生成特性。
- 运行时需配置包含上述实体的 `DbContext` 并注册 `IEntityStore<>`。

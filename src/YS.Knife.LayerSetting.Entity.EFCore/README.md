# YS.Knife.LayerSetting.Entity.EFCore

分层设置（LayerSetting）模块的 EF Core 实体定义包，提供设置组与设置属性的数据库存储模型。

## 主要功能

- `SettingEntity`：设置组实体，继承 `BaseEntity<Guid>`：
  - `Name`（最长 64）：设置组名称。
  - `Group`（最长 64，必填，唯一索引）：设置组标识。
  - `Description`（最长 256，可空）：描述。
  - `RoleProviders`：角色提供者名称数组，决定该设置组参与值计算的逻辑角色来源。
  - `Properties`：设置属性集合（导航属性）。
- `SettingPropertyEntity`：设置属性实体：
  - `SettingId`：外键，指向所属设置组；`(SettingId, Key)` 唯一索引。
  - `Key`（最长 64，必填）：属性键。
  - `Name`、`Description`：显示名与描述。
  - `IsArray`：是否为数组值。
  - `Type`（最长 64，必填）：值类型标识。
  - `DataSource`（最长 1024，可空）：数据源标识（如数据项名称）。
  - `Config`（`[JsonContent]`）：以 JSON 列存储的扩展配置。

## 使用示例

```csharp
public class AppDbContext : DbContext
{
    public DbSet<SettingEntity> Settings => Set<SettingEntity>();
    public DbSet<SettingPropertyEntity> SettingProperties => Set<SettingPropertyEntity>();
}

// 加载设置组及其属性
var setting = await dbContext.Settings
    .Include(s => s.Properties)
    .SingleAsync(s => s.Group == "mail");
```

## 依赖说明

- `YS.Knife.EFCore`：提供 `BaseEntity<TKey>`（经 `YS.Knife.Entity`）与 `[JsonContent]` 等 EF Core 扩展能力。
- `Microsoft.EntityFrameworkCore`：`[Index]` 特性支持。
- 运行时使用层：`YS.Knife.LayerSetting.Impl.EFCore`。

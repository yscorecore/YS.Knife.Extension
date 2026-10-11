# YS.Knife.EnumCode.Loader.Entity.EFCore

枚举码表（EnumCode）的 EF Core 实体定义，用于将码表存储到数据库，配合 `YS.Knife.EnumCode.Loader.Impl.EFCore` 从数据库加载码表数据。

## 主要功能

- `EnumCodeEntity<T>`：码表实体（泛型主键）
  - 字段：`Kind`（分类，最长 64）、`Key`（整型键）、`Name`（名称）、`Display`（显示名）、`Description`、`Group`（分组）、`Order`（排序）
  - `Kind + Key` 唯一索引
  - 实现 `ISortableEntity`（可排序）、`ISoftDeleteEntity`（逻辑删除，含 `IsDeleted`）
  - 继承 `BaseEntity<T>`（YS.Knife.EntityBase 提供）

## 使用示例

在 DbContext 中加入实体：

```csharp
public class AppDbContext : DbContext
{
    public DbSet<EnumCodeEntity<long>> EnumCodes { get; set; }
}
```

典型数据：

| Kind        | Key | Name    | Display | Order |
|-------------|-----|---------|---------|-------|
| orderStatus | 1   | Pending | 待支付  | 1     |
| orderStatus | 2   | Paid    | 已支付  | 2     |

配合 `IEnumCodeService` 聚合后，数据库中的码表会与程序集枚举加载器的结果合并（同名同键时按加载器优先级覆盖）。

## 依赖说明

- `YS.Knife.EFCore`：EF Core 基础（`BaseEntity<T>`、`ISortableEntity`、`ISoftDeleteEntity` 等基础设施）。
- 本包仅包含实体定义；数据库加载器实现见 `YS.Knife.EnumCode.Loader.Impl.EFCore`，服务编排见 `YS.Knife.EnumCode.Impl.Default`。

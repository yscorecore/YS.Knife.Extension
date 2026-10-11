# YS.Knife.KeyValue.Entity.EFCore

键值存储的 Entity Framework Core 实体定义，提供键值数据表 `KeyValueEntity<T>` 的实体模型，供基于 EF Core 的键值存储实现（`YS.Knife.KeyValue.Impl.EFCore`）使用。

## 主要功能

定义泛型键值实体：

```csharp
[Index(nameof(Key), IsUnique = true)]
public class KeyValueEntity<T> : BaseEntity<T>
{
    [Required]
    [StringLength(128)]
    public string Key { get; set; }   // 键，最长 128，唯一索引
    public string Value { get; set; } // 值
    public bool KeepString { get; set; } // 是否按字符串原样保存
}
```

- 继承 `BaseEntity<T>`，主键类型由泛型参数 `T` 指定（如 `Guid` / `string`）
- `Key` 字段带唯一索引，保证键不重复
- `KeepString` 与 `IKeyValueService.SetValue` 的 `keepString` 参数对应

## 使用方式

在自己的 `DbContext` 中挂载实体：

```csharp
public class AppDbContext : DbContext
{
    public DbSet<KeyValueEntity<Guid>> KeyValues => Set<KeyValueEntity<Guid>>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
}
```

配合 `YS.Knife.KeyValue.Impl.EFCore` 即可将键值数据持久化到数据库；如需 HTTP 接口再引入 `YS.Knife.KeyValue.Api.AspnetCore`。

## 依赖

- `YS.Knife.EFCore`（项目引用）：`BaseEntity<T>` 等 EF Core 基础设施
- `Microsoft.EntityFrameworkCore`（经 `YS.Knife.EFCore` 传递）

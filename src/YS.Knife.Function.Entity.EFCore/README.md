# YS.Knife.Function.Entity.EFCore

功能点（Function）模块的 EF Core 实体定义包，提供功能点树形结构的数据库存储模型。

## 主要功能

- `FunctionEntity`：功能点实体，继承 `BaseEntity<Guid>`，字段包括：
  - `Type`（最长 16，必填）：功能点类型。
  - `AppId`（最长 64，必填）：所属应用 ID。
  - `Name`（最长 64）：功能点名称。
  - `Code`（最长 128，必填）：功能点编码。
  - `Description`（最长 256，可空）：描述。
  - `ParentCode`（最长 128，可空）：父级功能点编码，用于构建树形结构。
  - `Sequence`：排序序号。
  - `Config`（`[JsonContent]`）：以 JSON 列存储的扩展配置字典。
- 索引定义：`AppId` 普通索引、`(AppId, Code)` 唯一索引、`(AppId, ParentCode)` 普通索引。

## 使用示例

```csharp
public class AppDbContext : DbContext
{
    public DbSet<FunctionEntity> Functions => Set<FunctionEntity>();
}

// 查询某应用下的顶层功能点
var roots = await dbContext.Functions
    .Where(f => f.AppId == "admin" && f.ParentCode == null)
    .OrderBy(f => f.Sequence)
    .ToListAsync();
```

## 依赖说明

- `YS.Knife.EntityBase`：提供 `BaseEntity<TKey>` 等实体基类。
- `YS.Knife.EFCore`：提供 `[JsonContent]` 等 EF Core 扩展能力。
- `Microsoft.EntityFrameworkCore`：`[Index]` 等特性支持。

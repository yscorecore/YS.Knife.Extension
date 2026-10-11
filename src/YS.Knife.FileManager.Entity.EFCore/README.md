# YS.Knife.FileManager.Entity.EFCore

云文件管理的 EF Core 实体定义层，提供文件/目录的通用实体模型。

## 主要功能

- 提供泛型实体 `FileEntity<T>`（`T : struct`），继承 `BaseEntity<T>` 并实现 `ISoftDeleteEntity`
- 字段说明：
  - `Name`：名称（最长 128，必填）
  - `Extension`：扩展名（最长 8）
  - `Key` / `Url`：存储键与访问地址
  - `Size`：文件大小
  - `Owner`：归属者/逻辑角色（最长 64，必填）
  - `ParentId` / `Parent`：父级目录（自引用，支持树形结构）
  - `IsFolder`：是否为目录
  - `IsDeleted`：软删除标记
- 索引定义：`Owner` 普通索引；`Owner + ParentId + Name` 唯一索引（同一归属同一目录下名称唯一）

## 使用示例

在业务 `DbContext` 中使用：

```csharp
public class AppDbContext : DbContext
{
    public DbSet<FileEntity<Guid>> Files => Set<FileEntity<Guid>>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<FileEntity<Guid>>();
    }
}
```

通常无需直接引用本层进行编码，实际业务由 `YS.Knife.FileManager.Impl.EFCore` 基于本实体实现。

## 依赖说明

- 项目引用 `YS.Knife.EFCore`：提供 `BaseEntity<T>`、`ISoftDeleteEntity` 等基础类型
- 基于 `Microsoft.EntityFrameworkCore`

目标框架：.NET 6.0。

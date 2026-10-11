# YS.Knife.CodeMapper.Entity.EFCore

CodeMapper（编码映射）模块的 EF Core 实体定义包。

## 主要功能

- 定义编码映射实体 `CodeMapperEntity<T>`，用于持久化"源编码 → 目标编码"的映射关系
- 内置索引：`Group` 普通索引；`(Group, SourceCode)` 唯一索引，保证同组内源编码唯一

## 实体说明

```csharp
[Index(nameof(Group))]
[Index(nameof(Group), nameof(SourceCode), IsUnique = true)]
public class CodeMapperEntity<T> : BaseEntity<T> where T : notnull
{
    public string Group { get; set; }        // 映射分组（必填，最长 128）
    public string SourceCode { get; set; }   // 源编码（最长 64）
    public string? SourceName { get; set; }  // 源名称（最长 64）
    public string? TargetCode { get; set; }  // 目标编码（最长 64）
    public string? TargetName { get; set; }  // 目标名称（最长 64）
}
```

## 快速开始

在自己的 `DbContext` 中注册该实体（通常由实现包 `YS.Knife.CodeMapper.Impl.EFCore` 通过 `IEntityStore<CodeMapperEntity<Guid>>` 访问）：

```csharp
public class AppDbContext : DbContext
{
    public DbSet<CodeMapperEntity<Guid>> CodeMappers { get; set; }
}
```

## 依赖说明

- 目标框架：.NET 6（`net6.0`）
- `YS.Knife.EFCore`：提供 `BaseEntity<T>` 等 EF Core 基础设施
- `Microsoft.EntityFrameworkCore`（经 `YS.Knife.EFCore` 传递）

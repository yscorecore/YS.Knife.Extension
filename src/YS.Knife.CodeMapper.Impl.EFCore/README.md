# YS.Knife.CodeMapper.Impl.EFCore

CodeMapper（编码映射）模块基于 Entity Framework Core 的实现。

## 主要功能

- `CodeMapperService`：`ICodeMapperService` 的 EF Core 实现
  - 按分组 + 源编码批量查询映射
  - 可选地将未登记的源编码自动写入数据库（`autoSyncSourceData`），供后续人工维护目标编码
- `CodeMapperManagerService`：`ICodeMapperManagerService<Guid>` 的实现
  - 分页查询映射关系（支持 `LimitQueryInfo` 查询条件）
  - 批量保存目标编码/名称（基于 FlyTiger Mapper 自动完成 DTO 与实体的双向映射）
- 两个服务均通过 FlyTiger `[Service]` 特性自动注册到 DI 容器

## 快速开始

1. 在 `DbContext` 中注册实体（来自 `YS.Knife.CodeMapper.Entity.EFCore`）：

```csharp
public class AppDbContext : DbContext
{
    public DbSet<CodeMapperEntity<Guid>> CodeMappers { get; set; }
}
```

2. 注入接口使用：

```csharp
using YS.Knife.CodeMapper;

public class ProductSyncService
{
    private readonly ICodeMapperService mapperService;
    private readonly ICodeMapperManagerService<Guid> managerService;

    public ProductSyncService(ICodeMapperService mapperService,
        ICodeMapperManagerService<Guid> managerService)
    {
        this.mapperService = mapperService;
        this.managerService = managerService;
    }

    public async Task<IEnumerable<string>> GetInternalCodesAsync(
        IEnumerable<ExternalProduct> products, CancellationToken token)
    {
        var results = await mapperService.MapAll(
            products, "Product", p => p.ExternalCode, p => p.ExternalName,
            autoInsertSourceData: true, token);

        return results.Where(r => r.Mapped).Select(r => r.TargetCode!);
    }

    // 后台维护：分页查询并保存目标映射
    public async Task MaintainAsync(CancellationToken token)
    {
        var page = await managerService.QueryPagedList(new LimitQueryInfo(), token);
        foreach (var item in page.Items)
        {
            // 为未映射的记录设置目标编码
            await managerService.SaveMappers(new[]
            {
                new ICodeMapperManagerService<Guid>.UpdateTargetMapperReq
                {
                    Id = item.Id,
                    TargetCode = "INT-001",
                    TargetName = "内部产品"
                }
            }, token);
        }
    }
}
```

## 依赖说明

- 目标框架：.NET 6（`net6.0`）
- `YS.Knife.CodeMapper.Core`：抽象接口
- `YS.Knife.CodeMapper.Entity.EFCore`：实体定义（实现内部固定使用 `CodeMapperEntity<Guid>`）
- `Microsoft.EntityFrameworkCore`、`FlyTiger`、`YS.Knife.Entity`（`IEntityStore<T>`）

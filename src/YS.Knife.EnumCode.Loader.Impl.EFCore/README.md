# YS.Knife.EnumCode.Loader.Impl.EFCore

基于 Entity Framework Core 的枚举码（EnumCode）加载器与管理器实现，从数据库中读取枚举码，并支持将程序集中定义的枚举同步到数据库。

## 主要功能

- `EFCodeLoader`：实现 `ICodeLoader` 接口，通过 `IEntityStore<EnumCodeEntity<Guid>>` 从数据库读取全部枚举码，按 `Kind` 分组、按 `Order` 排序返回（`Priority` 为 10000）。
- `EFCodeManager`：提供 `SyncEnumCodeFromAssemblies` 方法，扫描配置中指定程序集内的公共枚举类型，将其枚举值以“追加”模式同步到数据库（不删除已有数据）。
- `EnumCodeOptions`：配置项，指定需要扫描的程序集名称数组 `Assemblies`。

## 使用示例

```csharp
// appsettings.json 中配置需要扫描的程序集
{
  "EnumCodeOptions": {
    "Assemblies": [ "MyApp.Enums", "MyApp.Domain" ]
  }
}

// 服务通过 [Service] 特性自动注册（YS.Knife 自动注册机制）
// 注入并使用：
public class StartupService
{
    private readonly EFCodeManager codeManager;

    public StartupService(EFCodeManager codeManager)
    {
        this.codeManager = codeManager;
    }

    public async Task InitAsync(CancellationToken cancellationToken)
    {
        // 将程序集中定义的枚举同步到数据库
        await codeManager.SyncEnumCodeFromAssemblies(cancellationToken);
    }
}

// IEnumCodeService 的枚举码来源之一即为本项目的数据库加载器
public class EnumConsumer
{
    private readonly ICodeLoader codeLoader;

    public async Task<Dictionary<string, List<IEnumCodeService.CodeInfo>>> LoadAllAsync()
    {
        return await codeLoader.AllCodes();
    }
}
```

## 依赖说明

- `YS.Knife.EnumCode.Core`：提供 `ICodeLoader`、`IEnumCodeService.CodeInfo`、`AssemblyEnumCodes` 等抽象。
- `YS.Knife.EnumCode.Loader.Entity.EFCore`：提供 `EnumCodeEntity<Guid>` 实体定义。
- `YS.Knife.Core`：提供 `[Service]`、`[AutoConstructor]`、`[Options]`、`[Mapper]` 等源生成特性。
- 运行时需要在上下文中配置包含 `EnumCodeEntity<Guid>` 的 EF Core `DbContext`，并注册对应的 `IEntityStore<>`。

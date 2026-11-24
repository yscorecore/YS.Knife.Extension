# KnifeHost 迁移指南

本文档记录 `YS.Knife.Host` 中 `KnifeHost` 的一次破坏性 API 迁移，供依赖 `KnifeHost` 的项目（继承 `KnifeHost` 或 override 其虚拟方法的测试/示例/宿主程序）参照更新。

> 迁移提交：`fc83feb refactor(host): use Host.CreateApplicationBuilder instead of CreateDefaultBuilder`

---

## 1. 背景

`KnifeHost` 从基于 `Host.CreateDefaultBuilder`（`IHostBuilder` + 回调 lambda）的构建方式，迁移到 `Host.CreateApplicationBuilder`（直接操作 builder 属性）。同时移除了所有虚拟方法中不再需要的 `HostBuilderContext` 参数。

### 为什么迁移

- `Host.CreateApplicationBuilder`（.NET 7+）直接暴露 `Configuration`、`Environment`、`Logging`、`Services` 等属性，代码更直接、更少嵌套。
- 旧实现中 `HostBuilderContext` 作为参数在回调间传递，实际大多数 override 并不使用它，属于冗余噪音。

---

## 2. 变更总览

### 2.1 宿主构建方法

| 项 | 迁移前 | 迁移后 |
|----|--------|--------|
| 方法 | `protected virtual IHostBuilder CreateHostBuilder(string[] args)` | `protected virtual IHost CreateHost(string[] args)` |
| 返回类型 | `IHostBuilder` | `IHost` |
| 构建 | `Host.CreateDefaultBuilder(args).ConfigureXxx(...)` | `Host.CreateApplicationBuilder(args)` → `builder.Build()` |

迁移前：

```csharp
protected virtual IHostBuilder CreateHostBuilder(string[] args)
{
    return Host.CreateDefaultBuilder(args)
        .ConfigureLogging(OnConfigureLogging)
        .ConfigureAppConfiguration((ctx, builder) =>
        {
            this.OnConfigureAppConfiguration(ctx, builder);
            this.InjectInternalConfigurations(ctx, builder);
        })
        .ConfigureServices((ctx, services) =>
        {
            this.OnConfigureCustomService(ctx, services);
            this.OnConfigureInternalServices(ctx, services);
        });
}
```

迁移后：

```csharp
protected virtual IHost CreateHost(string[] args)
{
    var builder = Host.CreateApplicationBuilder(args);

    this.OnConfigureLogging(builder.Logging);
    this.OnConfigureAppConfiguration(builder.Configuration);
    this.InjectInternalConfigurations(builder.Configuration);

    builder.Services.AddAllKnifeServices(builder.Configuration, ShouldFilterType);
    this.OnConfigureCustomService(builder.Services);
    this.OnConfigureInternalServices(builder.Services);
    this.InjectInternalServices(builder.Services);

    return builder.Build();
}
```

### 2.2 虚拟方法签名（移除 `HostBuilderContext`）

| 方法 | 迁移前 | 迁移后 |
|------|--------|--------|
| `OnConfigureInternalServices` | `(HostBuilderContext, IServiceCollection)` | `(IServiceCollection)` |
| `OnConfigureCustomService` | `(HostBuilderContext, IServiceCollection)` | `(IServiceCollection)` |
| `OnConfigureLogging` | `(HostBuilderContext, ILoggingBuilder)` | `(ILoggingBuilder)` |
| `OnConfigureAppConfiguration` | `(HostBuilderContext, IConfigurationBuilder)` | `(IConfigurationBuilder)` |

> 私有辅助方法 `InjectInternalServices` / `InjectInternalConfigurations` 也同步移除了 `HostBuilderContext` 参数（属于内部实现，一般不涉及外部 override）。

---

## 3. 依赖项目如何迁移

如果你的项目继承 `KnifeHost` 并 override 了上述任一方法，需要做以下修改：

### 3.1 删除方法签名中的 `HostBuilderContext` 参数

迁移前：

```csharp
using Microsoft.Extensions.Hosting; // 仅为 HostBuilderContext

public class MyTest : KnifeHost
{
    protected override void OnConfigureCustomService(HostBuilderContext builder, IServiceCollection services)
    {
        base.OnConfigureCustomService(builder, services);
        services.AddXxx();
    }
}
```

迁移后：

```csharp
public class MyTest : KnifeHost
{
    protected override void OnConfigureCustomService(IServiceCollection services)
    {
        base.OnConfigureCustomService(services);
        services.AddXxx();
    }
}
```

### 3.2 清理不再需要的 using

若 `using Microsoft.Extensions.Hosting;` 仅为引入 `HostBuilderContext` 而添加，可一并删除。

### 3.3 现有已迁移示例

仓库内 4 个 override `OnConfigureCustomService` 的测试类已完成迁移，可作为参考：

- [MapperTest.cs](../test/YS.Knife.Mapper.Impl.FlyTiger.IntegrationTest/MapperTest.cs)
- [HangfireJobServiceTest.cs](../test/YS.Knife.Job.Impl.Hangfire.UnitTest/HangfireJobServiceTest.cs)
- [NPOIExportTest.cs](../test/YS.Knife.Export.Impl.NPOI.IntegrationTest/NPOIExportTest.cs)
- [ClosedXmlTest.cs](../test/YS.Knife.Export.ClosedXml.IntegrationTest/ClosedXmlTest.cs)

其余继承 `KnifeHost` 但未 override 上述方法的项目（如 [MetadataServiceTest.cs](../test/YS.Knife.Metadata.Impl.Mvc.IntegrationTest/MetadataServiceTest.cs) 等）无需改动。

---

## 4. 相关注意事项

### 4.1 `Microsoft.Extensions.Hosting` 包版本下限提升

`Host.CreateApplicationBuilder` 是 .NET 7+ 的 API，旧版 `Microsoft.Extensions.Hosting` 包（如 6.0）不包含。`YS.Knife.Host.csproj` 已将包引用下限从 `[6.0.0,99.0.0)` 提升为 `[8.0.0,99.0.0)`：

```xml
<PackageReference Include="Microsoft.Extensions.Hosting" Version="[8.0.0,99.0.0)" />
```

### 4.2 `KnifeWebHost` 已移除（monorepo 迁移）

旧的外部包 `YS.Knife.Hosting` 中曾提供 `KnifeWebHost`（Web 宿主，自动注册 ASP.NET Core MVC 服务）。monorepo 迁移（`63e04a7`）后仅保留了通用 `KnifeHost`，`KnifeWebHost` 未迁移。

如果你的项目依赖 `KnifeWebHost` 提供的 MVC 服务（如 `IModelMetadataProvider`），需自行注册。例如 `YS.Knife.Metadata.Impl.Mvc` 在 `ServiceRegister` 中：

```csharp
services.AddMvcCore().AddDataAnnotations();
```

### 4.3 `IServiceCollection` 仍会被注册

`OnConfigureInternalServices` 仍会注册 `KnifeHost` 实例与 `IServiceCollection`，供开发模式的调试信息输出（`LogDependencyInjectionServicesInfo`）使用，行为与迁移前一致。

---

## 5. 验证

迁移完成后建议执行：

```bash
dotnet build YS.Knife.Extensions.sln --nologo
dotnet test YS.Knife.Extensions.sln --nologo
```

编译失败通常是某处还残留旧的 `HostBuilderContext` 签名；可用以下命令全局搜索确认：

```bash
rg "HostBuilderContext|CreateHostBuilder" src test
```

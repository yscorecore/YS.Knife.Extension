# YS.Knife.Version.Core

应用版本信息的抽象层，定义版本查询服务接口及版本信息模型。

## 主要功能

- `IVersionService`：版本信息查询服务接口
- `VersionInfo`：版本信息模型（构建时间、程序集版本、文件版本、信息版本）
- `BuildTimeAttribute`：程序集级特性，用于在编译时把构建时间写入程序集元数据

## 快速开始

在程序集（通常是入口程序集）上声明构建时间：

```csharp
using YS.Knife.Version;

[assembly: BuildTime("2024-06-01 10:30:00")]
```

业务代码中注入接口获取版本信息（实现由 `YS.Knife.Version.Impl.Default` 提供）：

```csharp
public class VersionController
{
    private readonly IVersionService versionService;

    public VersionController(IVersionService versionService)
    {
        this.versionService = versionService;
    }

    public VersionInfo Get() => versionService.GetVersionInfo();
    // VersionInfo 包含：
    //   BuildTime                     构建时间（来自 BuildTimeAttribute）
    //   AssemblyVersion               程序集版本
    //   AssemblyFileVersion           文件版本
    //   AssemblyInformationalVersion  信息版本
}
```

## 接口说明

```csharp
public interface IVersionService
{
    VersionInfo GetVersionInfo();
}
```

## 依赖说明

- 目标框架：.NET 6（`net6.0`）
- 无第三方运行时依赖
- 实现层请参考 `YS.Knife.Version.Impl.Default`

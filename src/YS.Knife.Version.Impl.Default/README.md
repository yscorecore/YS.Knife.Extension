# YS.Knife.Version.Impl.Default

`IVersionService` 的默认实现，通过反射读取入口程序集的元数据返回版本信息。

## 主要功能

- `VersionService`：默认版本服务，读取入口程序集（`Assembly.GetEntryAssembly()`）的以下信息：
  - `BuildTimeAttribute` 中记录的构建时间（未设置时返回 `DateTime.UnixEpoch`）
  - `AssemblyVersionAttribute` 程序集版本
  - `AssemblyFileVersionAttribute` 文件版本
  - `AssemblyInformationalVersionAttribute` 信息版本
- 以 Singleton 生命周期自动注册到 DI 容器（FlyTiger `[Service]` 特性）

## 快速开始

1. 在入口项目（如 Web 应用）中声明构建时间：

```csharp
[assembly: YS.Knife.Version.BuildTime("2024-06-01 10:30:00")]
```

2. 直接注入 `IVersionService` 使用：

```csharp
using YS.Knife.Version;

public class AppInfoService
{
    private readonly IVersionService versionService;

    public AppInfoService(IVersionService versionService)
    {
        this.versionService = versionService;
    }

    public void PrintVersion()
    {
        var info = versionService.GetVersionInfo();
        Console.WriteLine($"构建时间：{info.BuildTime}");
        Console.WriteLine($"程序集版本：{info.AssemblyVersion}");
        Console.WriteLine($"文件版本：{info.AssemblyFileVersion}");
        Console.WriteLine($"信息版本：{info.AssemblyInformationalVersion}");
    }
}
```

## 依赖说明

- 目标框架：.NET 6（`net6.0`）
- `YS.Knife.Version.Core`：抽象接口与模型
- `YS.Knife.Core`、`FlyTiger`：服务自动注册

> 注意：实现基于入口程序集，若需要在类库或测试环境中使用，请确保入口程序集包含相应的版本元数据。

# YS.Knife.Settings.Core

设置（Settings）功能的抽象层，定义通用的设置读写服务接口与设置分组信息抽象。

## 主要功能

- `ISettingsService`：统一的设置读写接口，支持按键获取/保存设置值
- `IBaseSettingInfo`：设置分组信息抽象，通过 `GetGroupName` 标识设置所属分组
- 内置两个示例分组类：`UserSetting`、`SchoolSetting`

## 核心接口

```csharp
public interface ISettingsService
{
    Task<string> GetSetting<T>(string key);
    Task SetSetting<T>(string key, T value);
}

public interface IBaseSettingInfo
{
    Task<string> GetGroupName();
}
```

## 使用示例

通过依赖注入获取 `ISettingsService`（需配合具体实现使用）：

```csharp
public class MyService
{
    private readonly ISettingsService settingsService;

    public MyService(ISettingsService settingsService)
    {
        this.settingsService = settingsService;
    }

    public async Task SaveAndReadAsync()
    {
        // 保存设置
        await settingsService.SetSetting("theme", "dark");

        // 读取设置
        var theme = await settingsService.GetSetting<string>("theme");
    }
}
```

## 依赖说明

- 目标框架：`net8.0`
- 无外部包依赖，仅包含接口与基础类型定义

本包只定义抽象，具体存储实现（如数据库、配置文件、分布式缓存）由其他实现包提供。

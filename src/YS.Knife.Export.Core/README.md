# YS.Knife.Export.Core

分步式数据导出服务抽象层，定义导出流程的核心接口与数据模型（元数据、列定义、数据批次、导出令牌），不包含具体实现。

## 主要功能

- `IExportService`：导出服务接口，定义"开始 → 追加数据 → 结束/取消"的完整导出生命周期。
- 数据模型：
  - `EntityMetadata`：一个数据表（Sheet）的元数据，包含名称、显示名和列集合。
  - `EntityColumnMetadata`：列元数据（名称、显示名、宽度、数据类型）。
  - `EntityData`：一批待写入的数据（`List<Dictionary<string, object>>`）。
  - `ExportToken`：导出令牌（`Token` + 过期秒数 `ExpiredIn`）。
  - `DataType`：列数据类型枚举（`String` / `Number` / `Boolean` / `DateTime`）。

## 接口定义

```csharp
public interface IExportService
{
    Task<ExportToken> BeginExport(EntityMetadata[] metadatas); // 开始导出
    Task Export(Guid token, EntityData data);                  // 追加一批数据
    Task<bool> CancelExport(Guid token);                       // 取消导出
    Task<Stream> EndExport(Guid token);                        // 结束导出并获取文件流
}
```

## 使用示例

```csharp
public class UserExportService
{
    private readonly IExportService exportService;

    public UserExportService(IExportService exportService)
    {
        this.exportService = exportService;
    }

    public async Task<Stream> ExportUsersAsync()
    {
        var token = await exportService.BeginExport(new[]
        {
            new EntityMetadata
            {
                Name = "users",
                DisplayName = "用户",
                Columns = new List<EntityColumnMetadata>
                {
                    new() { Name = "Name", DisplayName = "姓名" },
                    new() { Name = "RegisterTime", DisplayName = "注册时间", DataType = DataType.DateTime }
                }
            }
        });

        await exportService.Export(token.Token, new EntityData
        {
            Name = "users",
            Datas = rows
        });

        return await exportService.EndExport(token.Token);
    }
}
```

## 说明

- 本项目仅为抽象层（Core）。配套的基类实现见 `YS.Knife.Export.Impl.Base`（`BaseExportService`，处理令牌、缓存与临时文件生命周期，文件格式写入留给派生类）。
- Web API 暴露层见 `YS.Knife.Export.Api.AspnetCore`。

## 依赖

无第三方包依赖，目标框架 .NET 6。

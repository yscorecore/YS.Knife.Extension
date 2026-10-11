# YS.Knife.Import.Core

数据导入功能的抽象层，定义导入服务接口 `IImportService` 及相关的数据传输对象，不包含任何具体 Excel 解析实现。

## 主要功能

### 核心接口 `IImportService`

```csharp
public interface IImportService
{
    // 开始导入：接收文件流，返回带过期时间的 token
    Task<ImportToken> BeginImport(Stream data, string dataFileName);
    // 按元数据映射读取表格数据
    Task<Dictionary<string, List<Dictionary<string, object>>>> ReadData(
        Guid token, EntityMetadata[] entityMetadata);
    // 读取各工作表的列头
    Task<Dictionary<string, List<ColumnInfo>>> ReadColumns(Guid token);
    // 结束导入并清理资源
    Task<bool> EndImport(Guid token);
}
```

### DTO 定义

- `ImportToken`：导入令牌（`Token`、`ExpiredIn` 秒数）
- `ColumnInfo`：列头信息（`ColumnName`）
- `EntityMetadata`：实体（工作表）元数据，支持 `Name` / `Position` / `Alias` 定位工作表
- `EntityColumnMetadata`：列元数据，支持 `Name` / `Position` / `Alias` / `DataType` / `Optional`
- `DataType` 枚举：`String`、`Boolean`、`DateTime`、`Number`

## 使用方式

本项目通常不直接使用，而是：

- 作为 `YS.Knife.Import.Impl.Base` / `YS.Knife.Import.Impl.ClosedXml` / `YS.Knife.Import.Impl.NPOI` 等实现包的抽象依赖
- 在业务代码中面向 `IImportService` 编程，通过依赖注入获取实现

```csharp
public class DemoService
{
    private readonly IImportService importService;

    public DemoService(IImportService importService) => this.importService = importService;

    public async Task ImportAsync(Stream fileStream, string fileName)
    {
        var token = await importService.BeginImport(fileStream, fileName);
        var columns = await importService.ReadColumns(token.Token);
        // ... 构建 EntityMetadata 后读取数据
        await importService.EndImport(token.Token);
    }
}
```

## 依赖

- 无第三方依赖（`System.ComponentModel.DataAnnotations` 用于 DTO 校验）
- 实现包：`YS.Knife.Import.Impl.ClosedXml`（xlsx）或 `YS.Knife.Import.Impl.NPOI`（xls/xlsx）

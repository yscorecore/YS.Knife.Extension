# YS.Knife.Export.Impl.Base

`IExportService` 的抽象基类实现，封装分步导出的通用流程（令牌管理、过期控制、分布式缓存状态、临时文件生命周期），具体文件格式（Excel/CSV 等）的写入由派生类实现。

## 主要功能

- `BaseExportService`：实现 `IExportService` 的抽象基类：
  - `BeginExport`：生成令牌、保存导出状态到 `IDistributedCache`、调用 `WriteDataTitle` 写入表头。
  - `Export`：校验令牌，按名称匹配元数据，调用 `WriteData` 追加数据。
  - `EndExport`：返回临时文件只读流并清理缓存状态。
  - `CancelExport`：删除临时文件并清理缓存状态。
  - 令牌过期与不存在时抛出带错误码的异常（`001` Token已过期 / `002` Token不存在 / `003` 元数据不存在）。
- `ExportOptions`：配置项（`[Options]` 源生成器自动注册）：
  - `DataFolder`：临时文件目录，默认为系统临时目录。
  - `ExpiredIn`：令牌有效期（秒），默认 600，范围 1 ~ 86400。

## 使用示例

继承 `BaseExportService` 实现具体格式（如 Excel）：

```csharp
public class ExcelExportService : BaseExportService
{
    public ExcelExportService(ExportOptions exportOptions, IDistributedCache distributedCache)
        : base(exportOptions, distributedCache)
    {
    }

    protected override Task WriteDataTitle(string fileName, EntityMetadata[] metadatas)
    {
        // 创建 Excel 文件并写入每个 Sheet 的表头
    }

    protected override Task WriteData(string fileName, EntityMetadata metadata,
        int metadataIndex, List<Dictionary<string, object>> datas)
    {
        // 向指定 Sheet 追加数据行
    }
}
```

配置（appsettings.json 或代码）：

```json
{
  "Export": {
    "DataFolder": "D:/export-temp",
    "ExpiredIn": 600
  }
}
```

可重写的扩展点：

- `GetDataFileName(Guid token)`：自定义临时文件命名规则（默认 `{时间戳}_{token}`）。
- `SaveExportInfo` / `GetExportInfo` / `DeleteExportInfo`：自定义导出状态的存取（默认使用 `IDistributedCache`）。

## 说明

- 抽象接口 `IExportService` 定义于 `YS.Knife.Export.Core`；本项目是其基础实现（`*.Impl.Base`），仍需派生类补齐文件格式写入逻辑。
- HTTP 接口层见 `YS.Knife.Export.Api.AspnetCore`。

## 依赖

- `YS.Knife.Core`（`[AutoConstructor]`、`[Options]`、`[CodeExceptions]` 等源生成器）
- `Microsoft.Extensions.Caching.Distributed`
- 项目引用：`YS.Knife.Export.Core`、`YS.Knife.Extensions.DistributedCache`
- 目标框架：.NET 6+

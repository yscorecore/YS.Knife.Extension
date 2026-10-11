# YS.Knife.Export.Impl.ClosedXml

基于 ClosedXML 的导出服务实现，将数据导出为 `.xlsx` Excel 文件。

## 主要功能

- 提供 `ExportService`，继承自 `YS.Knife.Export.Impl.BaseExportService`，实现 `IExportService` 接口
- 通过 `[Service(Lifetime = ServiceLifetime.Singleton)]` 自动注册为单例服务（配合 YS.Knife 的依赖注入扫描机制）
- 创建多工作表 Excel 文件：按 `EntityMetadata` 生成表头（使用 `DisplayName`，支持列宽设置）
- 追加数据行：按导出令牌（token）对应的文件追加写入数据
- 生成 `.xlsx` 格式文件（重写了 `GetDataFileName` 扩展名）

## 快速开始

在宿主项目中引用本包后，通过 YS.Knife 的服务注册机制自动扫描注册，然后从 DI 中获取 `IExportService` 使用：

```csharp
public class MyService
{
    private readonly IExportService exportService;

    public MyService(IExportService exportService)
    {
        this.exportService = exportService;
    }

    public async Task ExportAsync()
    {
        // 1. 开始导出，传入实体元数据
        ExportToken token = await exportService.BeginExport(metadatas);

        // 2. 分批写入数据
        await exportService.WriteData(token.Token, "metadataName", datas);

        // 3. 结束导出并获取下载信息
        var result = await exportService.EndExport(token.Token);
    }
}
```

## 依赖说明

- `ClosedXML` (0.102.1)：Excel 文件读写
- `YS.Knife.Core`：提供 `[AutoConstructor]`、`[Service]` 等源生成特性
- 项目引用 `YS.Knife.Export.Impl.Base`：导出流程基类与 `EntityMetadata` 等模型

目标框架：.NET 6.0。

# YS.Knife.Documents.Preview.Impl.OfficeApps

`IDocumentOnlinePreviewService` 的 Office Apps 实现，计划基于微软 Office Online（Office Web Viewer）为 Office 文档生成在线预览地址。

## 主要功能

- `DocumentOnlinePreviewService`：实现 `YS.Knife.Documents.Preview.Core` 中的 `IDocumentOnlinePreviewService` 接口。

> 注意：当前版本的方法尚未实现（抛出 `NotImplementedException`），本项目目前仅为实现占位/骨架。

## 使用方式（规划）

```csharp
public class PreviewController : ControllerBase
{
    private readonly IDocumentOnlinePreviewService previewService;

    public PreviewController(IDocumentOnlinePreviewService previewService)
    {
        this.previewService = previewService;
    }

    [HttpGet("preview-url")]
    public Task<string> GetPreviewUrl(string fileUrl, CancellationToken ct)
    {
        return previewService.GetPreviewUrl(fileUrl, ct);
    }
}
```

## 说明

- 抽象接口定义在 `YS.Knife.Documents.Preview.Core` 项目中，本项目为其具体实现（命名规则：`*.Impl.Xxx` 为 `*.Core` 的实现）。
- 业务代码应始终依赖 `IDocumentOnlinePreviewService` 接口，而非本实现类。

## 依赖

- 项目引用：`YS.Knife.Documents.Preview.Core`
- 目标框架：.NET 6+

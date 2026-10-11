# YS.Knife.Documents.Preview.Core

文档在线预览抽象层，定义文档在线预览、在线格式转换、在线打印的服务接口，不包含具体实现。

## 主要功能

- `IDocumentOnlinePreviewService`：根据文件 URL 获取在线预览地址。
- `IDocumentOnlineConvertService`：根据文件 URL 获取转换为目标格式的地址。
- `IDocumentOnlinePrintService`：根据文件 URL 获取在线打印地址。

接口定义：

```csharp
public interface IDocumentOnlinePreviewService
{
    Task<string> GetPreviewUrl(string fileUrl, CancellationToken cancellationToken = default);
}

public interface IDocumentOnlineConvertService
{
    Task<string> GetConvertUrl(string fileUrl, string targetFromat, CancellationToken cancellationToken = default);
}

public interface IDocumentOnlinePrintService
{
    Task<string> GetPrintUrl(string fileUrl, CancellationToken cancellationToken = default);
}
```

## 使用示例

```csharp
public class AttachmentService
{
    private readonly IDocumentOnlinePreviewService previewService;

    public AttachmentService(IDocumentOnlinePreviewService previewService)
    {
        this.previewService = previewService;
    }

    public Task<string> GetAttachmentPreviewUrlAsync(string fileUrl, CancellationToken ct)
    {
        return previewService.GetPreviewUrl(fileUrl, ct);
    }
}
```

## 说明

本项目仅为抽象层（Core）。具体实现请引用实现项目（命名规则：`*.Impl.Xxx` 为 `*.Core` 的实现），例如：

- `YS.Knife.Documents.Preview.Impl.OfficeApps`：基于微软 Office Online Apps（Office Web Viewer）的预览实现。

## 依赖

无第三方包依赖，目标框架 .NET 6+。

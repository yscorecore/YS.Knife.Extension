# YS.Knife.FileStorage.Default

文件上传服务的默认实现，基于“文件分类（FileCategory）”配置驱动整个上传流程：校验 → 路径模板渲染 → 流拦截 → 存储 → 回调。

## 主要功能

- `FileUploadService`：实现 `IFileUploadService`，按分类规则处理上传
  - 校验文件大小（`MaxLength`）、扩展名白名单（`AllowExtensions`）、文件名合法性
  - 使用路径模板（如 `${category}/{now:yyyyMMdd}/{guid}${ext}`）生成存储 Key
  - 支持命名流拦截器（`IFileStreamInterceptor`），如压缩、加水印
  - 支持上传完成后的命名回调（`IFileUploadCallBack`）
  - 通过服务开关（ServiceSwitch）按分类的 `ServiceName` 选择具体的 `IFileStorageService`（本地/OSS/MinIO 等）
- `FileUploadWebService`：实现 `IFileUploadWebService`，直接从当前 HTTP 请求的表单中读取文件与参数完成上传，并向前端提供上传限制信息（`GetUploadInfo`）
- `TemplatePlaceholder`：模板占位符填充器，支持系统参数（`{now:yyyyMMdd}`）与用户参数（`${xxx}`），用户参数值限定为 `\w+`（字母数字下划线），防止路径注入

## 快速开始

服务通过 YS.Knife 的 `[Service]` 特性自动注册。在控制器中：

```csharp
[ApiController]
[Route("api/files")]
public class FilesController : ControllerBase
{
    private readonly IFileUploadWebService uploadWebService;

    public FilesController(IFileUploadWebService uploadWebService)
    {
        this.uploadWebService = uploadWebService;
    }

    [HttpPost("{category}")]
    public async Task<FileUploadObject> Upload(string category, CancellationToken token)
    {
        // 表单字段名默认为 FileCategory.FileFormName（"file"）
        // query 与 form 中的其他字段会作为用户参数参与路径模板渲染
        return await uploadWebService.Upload(category, token);
    }
}
```

编程方式上传（不依赖 HTTP 上下文）：

```csharp
var result = await fileUploadService.Upload(
    category: "avatar",
    stream: stream,
    streamFileName: "photo.jpg",
    streamLength: stream.Length,
    streamContentType: "image/jpeg",
    userArgs: new Dictionary<string, string> { ["userId"] = "1001" },
    cancellationToken: token);
```

## 依赖说明

- `YS.Knife.FileStorage.Core`：抽象接口与模型。
- `YS.Knife.FileStorage.CategoryProvider.Default`：默认的文件分类提供者。
- `YS.Knife.FileStorage.SystemArguments.Default`：默认系统参数提供者（如 `now`、`guid`）。
- `YS.Knife.ServiceSwitch`：按名称/配置开关选择存储实现。
- `Microsoft.AspNetCore.Mvc.Abstractions` (2.x)。

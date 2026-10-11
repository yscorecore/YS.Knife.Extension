# YS.Knife.FileStorage.Core

文件存储与上传的抽象层，定义了存储服务、上传服务、文件分类、流拦截器、回调与模板占位符等核心接口和模型。

## 主要功能

- 存储抽象 `IFileStorageService`：
  - `PutObject(key, content, metadata)`：写入文件流
  - `MoveObject(key, newKey)`：移动/重命名
  - `GetObject(key)` / `Exists(key)`：查询
- 上传抽象 `IFileUploadService`：
  - `Upload(category, stream, fileName, length, contentType, userArgs)`：按分类规则上传
  - 扩展方法：`UploadBytes`、`UploadBase64`、`UploadFile`（本地路径）
- `IFileUploadWebService`：面向 Web 的上传接口，`GetUploadInfo` 返回前端所需的上传限制信息
- `FileCategory`：文件分类配置，含存储服务名、表单字段名、路径模板（默认 `${category}/{now:yyyyMMdd}/{guid}${ext}`）、元数据、拦截器、大小限制（默认 4MB）、扩展名白名单与回调
- 扩展点：
  - `IFileCategoryProvider`：按名称提供分类配置
  - `IFileStreamInterceptor`：上传前处理文件流（压缩、水印等）
  - `IFileUploadCallBack`：上传完成回调
  - `ITemplatePlaceholder` / `ISystemArgProvider`：路径模板占位符填充与系统参数（如 `now`、`guid`）
- 模型：`FileObject`（Key/FileName/PublicUrl）、`FileUploadObject`（含大小、扩展名、原始文件名）、`ClientUploadInfo`

## 使用示例

```csharp
public class ReportService
{
    private readonly IFileUploadService uploadService;

    public ReportService(IFileUploadService uploadService)
        => this.uploadService = uploadService;

    public Task<FileObject> SaveReport(string path)
        => uploadService.UploadFile("report", path); // 使用扩展方法按本地路径上传
}
```

## 依赖说明

- `YS.Knife.Extensions`：`ContentTypeMappings` 等基础工具。
- 实现层：存储实现 `YS.Knife.FileStorage.Impl.Local` / `Impl.AliyunOss` / `Impl.Minio`；上传编排实现 `YS.Knife.FileStorage.Default`。

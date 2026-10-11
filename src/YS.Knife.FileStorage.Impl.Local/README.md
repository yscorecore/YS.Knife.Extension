# YS.Knife.FileStorage.Impl.Local

`IFileStorageService` 的本地磁盘实现，将上传的文件保存在 ASP.NET Core 站点的 `wwwroot` 目录下，并生成可直接通过 HTTP 访问的公共 URL。

## 主要功能

- 将文件流保存到本地 `wwwroot/{UploadRootFolder}` 目录（自动创建子目录）
- 支持文件移动（`MoveObject`）、查询（`GetObject`）、存在性检查（`Exists`）
- 自动生成公共访问地址：可配置固定域名 `PublicPoint`，未配置时根据当前 HTTP 请求的 Scheme/Host 动态拼接
- 基于 YS.Knife 的 `[Service]` / `[Options]` 源生成器，服务与配置自动注册

## 快速开始

1. 引用本包（同时会自动引入 `YS.Knife.FileStorage.Core`），项目中使用 YS.Knife 的服务自动注册机制，`LocalFileStorageService` 会以 `IFileStorageService` 注册到 DI 容器。

2. 在 `appsettings.json` 中配置 `UploadOptions`：

```json
{
  "Upload": {
    "UploadRootFolder": "upload",
    "PublicPoint": "https://cdn.example.com/upload"
  }
}
```

- `UploadRootFolder`：wwwroot 下的上传根目录，默认 `upload`。
- `PublicPoint`：公共访问地址前缀，可选；不配置时按当前请求地址生成 URL。

3. 使用：

```csharp
public class MyService
{
    private readonly IFileStorageService fileStorage;

    public MyService(IFileStorageService fileStorage)
    {
        this.fileStorage = fileStorage;
    }

    public async Task SaveAsync(Stream stream)
    {
        var obj = await fileStorage.PutObject("images/2024/a.png", stream,
            new Dictionary<string, object>());
        Console.WriteLine(obj.PublicUrl); // https://<host>/upload/images/2024/a.png
    }

    public async Task<bool> CheckAsync(string key)
        => await fileStorage.Exists(key);
}
```

## 依赖说明

- `YS.Knife.FileStorage.Core`：提供 `IFileStorageService`、`FileObject` 等抽象。
- `YS.Knife.Core`（2.0.2）：提供 `[Service]`、`[AutoConstructor]`、`[Options]` 等源生成器。
- 依赖 `IWebHostEnvironment` 与 `IHttpContextAccessor`，适用于 ASP.NET Core Web 项目。

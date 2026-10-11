# YS.Knife.FileStorage.Impl.Minio

基于 Minio 对象存储的 `IFileStorageService` 实现，兼容 S3 协议，提供文件上传、移动、查询和存在性检查。

## 主要功能

- 实现 `YS.Knife.FileStorage.Core` 中的 `IFileStorageService` 接口
- 上传文件流并附带自定义元数据（`PutObject`）
- 通过复制 + 删除实现对象移动（`MoveObject`）
- 检查对象是否存在（`Exists`，失败时记录日志并返回 `false`）
- 根据 key 生成对外访问的公共 URL（`GetObject`）
- `Endpoint` 支持 `http://` / `https://` 前缀，自动决定是否启用 SSL

## 快速开始

服务通过 `[Service]` 特性自动注册到依赖注入容器，配置 `MinioOptions` 后即可注入使用。

```json
// appsettings.json
{
  "Minio": {
    "Endpoint": "http://127.0.0.1:9000",
    "BucketName": "my-bucket",
    "AccessKeyId": "minioadmin",
    "AccessKeySecret": "minioadmin",
    "PublicPoint": "http://127.0.0.1:9000/my-bucket"
  }
}
```

```csharp
public class DemoService
{
    private readonly IFileStorageService fileStorage;

    public DemoService(IFileStorageService fileStorage)
    {
        this.fileStorage = fileStorage;
    }

    public async Task UploadAsync(Stream content)
    {
        var file = await fileStorage.PutObject(
            "uploads/a.png",
            content,
            new Dictionary<string, object> { ["Content-Type"] = "image/png" });

        Console.WriteLine(file.PublicUrl);

        if (await fileStorage.Exists(file.Key))
        {
            await fileStorage.MoveObject(file.Key, "archived/a.png");
        }
    }
}
```

## 配置项（MinioOptions）

| 属性 | 说明 |
| --- | --- |
| `Endpoint` | Minio 服务地址，可带 `http://` / `https://` 前缀（必填） |
| `BucketName` | Bucket 名称（必填） |
| `AccessKeyId` | 访问密钥 Id（必填） |
| `AccessKeySecret` | 访问密钥 Secret（必填） |
| `PublicPoint` | 对外访问地址前缀，用于拼接 `PublicUrl`（必填） |

配置节名称为 `Minio`（由 `[Options]` 按类名约定推导）。

## 依赖

- `Minio`（6.0.2）：Minio .NET 客户端
- `YS.Knife.Core`：`[Service]`、`[Options]`、`[AutoConstructor]` 等特性
- `YS.Knife.FileStorage.Core`（项目引用）：抽象接口层

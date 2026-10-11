# YS.Knife.FileStorage.Impl.AliyunOss

基于阿里云 OSS 的 `IFileStorageService` 实现，提供文件对象的上传、移动、查询和存在性检查。

## 主要功能

- 实现 `YS.Knife.FileStorage.Core` 中的 `IFileStorageService` 接口
- 上传文件到阿里云 OSS Bucket（`PutObject`）
- 同 Bucket 内复制并删除源对象实现移动（`MoveObject`）
- 检查对象是否存在（`Exists`）
- 根据 key 生成对外访问的公共 URL（`GetObject`）
- 通过 `[DictionaryKey("alioss")]` 注册为命名存储服务，可在多存储实现中按名称 `alioss` 选择

## 快速开始

服务通过 YS.Knife 的 `[Service]` 特性自动注册，配置好 `AliyunOssOptions` 即可注入使用。

```json
// appsettings.json
{
  "Oss": {
    "Endpoint": "https://oss-cn-hangzhou.aliyuncs.com",
    "BucketName": "my-bucket",
    "AccessKeyId": "<AccessKeyId>",
    "AccessKeySecret": "<AccessKeySecret>",
    "PublicPoint": "https://my-bucket.oss-cn-hangzhou.aliyuncs.com"
  }
}
```

```csharp
public class DemoService
{
    private readonly IFileStorageService fileStorage;

    public DemoService(IFileStorageService fileStorage)
    {
        this.fileStorage = fileStorage; // 按 dictionary key "alioss" 解析
    }

    public async Task UploadAsync(Stream content)
    {
        var file = await fileStorage.PutObject(
            "uploads/2024/a.png",
            content,
            new Dictionary<string, object> { ["Content-Type"] = "image/png" });

        Console.WriteLine(file.PublicUrl);

        var exists = await fileStorage.Exists(file.Key);
        await fileStorage.MoveObject(file.Key, "archived/a.png");
    }
}
```

## 配置项（AliyunOssOptions）

| 属性 | 说明 |
| --- | --- |
| `Endpoint` | OSS 服务端点（必填） |
| `BucketName` | Bucket 名称（必填） |
| `AccessKeyId` | 访问密钥 Id（必填） |
| `AccessKeySecret` | 访问密钥 Secret（必填） |
| `PublicPoint` | 对外访问地址前缀，用于拼接 `PublicUrl`（必填） |

配置节名称为 `Oss`（由 `[Options("Oss")]` 指定）。

## 依赖

- `Aliyun.OSS.SDK.NetCore`（2.13.0）：阿里云 OSS 官方 SDK
- `YS.Knife.Core`：`[Service]`、`[Options]` 等自动注册特性
- `YS.Knife.FileStorage.Core`（项目引用）：抽象接口层

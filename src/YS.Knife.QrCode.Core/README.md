# YS.Knife.QrCode.Core

二维码生成的抽象层，定义 `IQrCodeService` 接口及便捷扩展方法。

## 主要功能

- `IQrCodeService`：二维码生成服务接口，根据内容与尺寸生成二维码图片流
  - 内嵌 `QrCode` 记录类型：`Content`（内容）、`Size`（尺寸）
- `QrCodeServiceExtensions` 扩展方法：
  - `GenerateBarCode(content, size)`：以简单参数生成二维码流
  - `GenerateBarCodeToFile(content, size, outputFile)`：直接生成到文件

## 接口定义

```csharp
public interface IQrCodeService
{
    Task<Stream> GenerateQrCode(QrCode barcode);

    public record QrCode
    {
        public string Content { get; set; }
        public int Size { get; set; }
    }
}
```

## 使用示例

```csharp
public class MyService
{
    private readonly IQrCodeService qrCodeService;

    public MyService(IQrCodeService qrCodeService)
    {
        this.qrCodeService = qrCodeService;
    }

    public async Task Demo()
    {
        // 生成二维码图片流
        using Stream stream = await qrCodeService.GenerateBarCode("https://example.com", 20);

        // 直接输出到文件
        await qrCodeService.GenerateBarCodeToFile("https://example.com", 20, "qrcode.png");
    }
}
```

## 依赖说明

本包仅为抽象层，无任何第三方依赖，目标框架 net6.0。具体实现见：

- `YS.Knife.QrCode.Impl.QRCoder`：基于 QRCoder 库的实现

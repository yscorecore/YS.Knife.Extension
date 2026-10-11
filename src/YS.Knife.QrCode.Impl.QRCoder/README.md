# YS.Knife.QrCode.Impl.QRCoder

基于 [QRCoder](https://github.com/codebude/QRCoder) 库的 `IQrCodeService` 实现，输出 PNG 格式二维码。

## 主要功能

- 实现 `YS.Knife.QrCode.Core` 定义的 `IQrCodeService` 接口
- 使用 QRCoder 的 `PngByteQRCode` 生成 PNG 图片字节流（纠错级别 M）
- 自动服务注册（`[Service]` + `[AutoConstructor]`），引用本包后无需手动注册

## 快速开始

引用本包后，注入 `IQrCodeService` 即可使用：

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
        // 生成 PNG 二维码流
        using Stream stream = await qrCodeService.GenerateQrCode(
            new IQrCodeService.QrCode
            {
                Content = "https://example.com",
                Size = 20, // 每个模块的像素大小
            });

        // 或使用扩展方法直接写入文件
        await qrCodeService.GenerateBarCodeToFile("https://example.com", 20, "qrcode.png");
    }
}
```

## 依赖说明

- `YS.Knife.QrCode.Core`：提供 `IQrCodeService` 接口及扩展方法
- `YS.Knife.Core`：提供 `[Service]` / `[AutoConstructor]` 自动注册能力
- `QRCoder`（1.6.0）
- 目标框架：net6.0

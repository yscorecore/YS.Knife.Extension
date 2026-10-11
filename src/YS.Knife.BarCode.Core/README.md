# YS.Knife.BarCode.Core

条形码生成服务抽象层，定义 `IBarCodeService` 接口及便捷扩展方法，不包含具体生成实现。

## 主要功能

- `IBarCodeService`：条形码生成接口，根据内容、宽高生成条形码图片流。
- `IBarCodeService.BarCode`：请求模型（`Content` 内容、`Width` 宽度、`Height` 高度）。
- `BarCodeServiceExtensions`：扩展方法，支持直接传参生成、生成并写入文件。

## 使用示例

```csharp
public class LabelService
{
    private readonly IBarCodeService barCodeService;

    public LabelService(IBarCodeService barCodeService)
    {
        this.barCodeService = barCodeService;
    }

    public async Task<Stream> CreateLabelAsync(string content)
    {
        // 方式一：使用请求模型
        return await barCodeService.GenerateBarCode(new IBarCodeService.BarCode
        {
            Content = content,
            Width = 300,
            Height = 120
        });
    }

    public async Task SaveLabelAsync(string content, string path)
    {
        // 方式二：扩展方法直接生成到文件
        await barCodeService.GenerateBarCodeToFile(content, 300, 120, path);
    }
}
```

## 说明

本项目仅为抽象层（Core）。具体实现请引用实现项目，例如：

- `YS.Knife.BarCode.Impl.BarcodeLib`：基于 BarcodeLib + SkiaSharp 的 Code128 实现，服务通过 `[Service]` 特性自动注册到依赖注入容器。

## 依赖

无第三方包依赖，目标框架 .NET 6+。

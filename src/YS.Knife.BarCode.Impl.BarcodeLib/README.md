# YS.Knife.BarCode.Impl.BarcodeLib

基于 [BarcodeLib](https://github.com/barnhill/barcodelib)（BarcodeStandard）与 SkiaSharp 的 `IBarCodeService` 实现，生成 Code128 格式的条形码 JPEG 图片。

## 主要功能

- `BarCodeService`：实现 `IBarCodeService`，使用 Code128 编码生成黑底白字（黑条白底）条形码，输出 JPEG 格式图片流。
- 通过 `[Service]` 特性（YS.Knife.Core 源生成器）自动注册到依赖注入容器，无需手动注册。

## 快速开始

引用本项目后，`IBarCodeService` 即可被注入使用：

```csharp
public class GoodsController : ControllerBase
{
    private readonly IBarCodeService barCodeService;

    public GoodsController(IBarCodeService barCodeService)
    {
        this.barCodeService = barCodeService;
    }

    [HttpGet("barcode")]
    public async Task<IActionResult> GetBarCode(string code)
    {
        var stream = await barCodeService.GenerateBarCode(code, 400, 150);
        return File(stream, "image/jpeg");
    }
}
```

## 说明

- 抽象接口定义在 `YS.Knife.BarCode.Core` 项目中，本项目为其具体实现（命名规则：`*.Impl.Xxx` 为 `*.Core` 的实现）。
- 当前生成参数固定为：Code128 编码、不包含文字标签（`IncludeLabel = false`）、JPEG 质量 100。

## 依赖

- `BarcodeLib` 3.1.3
- `SkiaSharp`（BarcodeStandard 依赖）
- `YS.Knife.Core`（`[AutoConstructor]`、`[Service]` 源生成器）
- 项目引用：`YS.Knife.BarCode.Core`

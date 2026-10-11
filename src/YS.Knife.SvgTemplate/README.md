# YS.Knife.SvgTemplate

SVG 模板渲染引擎：将 SVG 文本中的占位符替换为上下文数据后，渲染为 PNG 图片流。

## 主要功能

- `TemplateEngine.GenerateImage(templateContent, context)`：
  - 占位符语法：`${{ name }}`（支持字母数字下划线键名，空白可选）
  - 上下文中找不到的键保留原样，不做替换
  - 使用 Svg.Skia 解析 SVG 并光栅化为 PNG，返回定位在起始位置的 `MemoryStream`

## 快速开始

```csharp
using YS.Knife.SvgTemplate;

var template = """
<svg xmlns="http://www.w3.org/2000/svg" width="200" height="80">
  <rect width="200" height="80" fill="#336699"/>
  <text x="100" y="45" fill="#fff" font-size="20" text-anchor="middle">${{ userName }}</text>
</svg>
""";

var engine = new TemplateEngine();
await using var pngStream = engine.GenerateImage(template, new Dictionary<string, object>
{
    ["userName"] = "张三"
});

await File.WriteAllBytesAsync("output.png", pngStream.ToArray());
```

适用于名片、海报、证书、二维码底图等“模板 + 数据”批量出图场景。

## 依赖说明

- `Svg.Skia`（5.0.0）：SVG 解析与 SkiaSharp 渲染。
- 无 YS.Knife 服务注册依赖，`TemplateEngine` 为普通类，可直接实例化。

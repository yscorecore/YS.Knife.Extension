using System.Data.SqlTypes;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using Svg;
using Svg.Skia;

namespace YS.Knife.SvgTemplate
{
    public class TemplateEngine
    {
        private static Regex regex = new Regex(@"\${{\s*(\w+)\s*}}", RegexOptions.Compiled);
        public SvgImage GenerateImage(string templateContent, IDictionary<string, object> context)
        {
            var fullSvgContent = regex.Replace(templateContent, match =>
            {
                var key = match.Groups[1].Value;
                if (context.TryGetValue(key, out var value))
                {
                    return value?.ToString() ?? string.Empty;
                }
                else
                {
                    return match.Value; // No replacement, keep original
                }
            });

            var svgDocument = SvgDocument.FromSvg<SvgDocument>(fullSvgContent);
            var svgImage = new SvgImage();
            svgImage.Load(svgDocument);
            using var skBitmap = svgImage.ToBitmap();
            // Encode to PNG
            using var data = skBitmap.Encode(SkiaSharp.SKEncodedImageFormat.Png, 100);
            var stream = new MemoryStream();
            data.SaveTo(stream);
            stream.Position = 0;
            return stream;
        }
    }
}

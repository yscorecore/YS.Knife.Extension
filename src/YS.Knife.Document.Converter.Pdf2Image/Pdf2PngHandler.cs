using PdfiumRaster;

namespace YS.Knife.Document.Converter.Pdf2Image
{
    [AutoConstructor]
    [Service(typeof(IDocTypeConvertHandler))]
    public partial class Pdf2PngHandler : DocTypeConvertHandlerBase<Pdf2PngOption>
    {
        public override string Name => "pdf2png";


        public override StreamBody Convert(StreamBody input, Pdf2PngOption options, CancellationToken token = default)
        {
            return PdfPageRenderer.Render(
                input,
                options.Dpi,
                options.Quality,
                PdfImageOutputFormat.Png,
                "image/png",
                "png",
                token);
        }
    }
}

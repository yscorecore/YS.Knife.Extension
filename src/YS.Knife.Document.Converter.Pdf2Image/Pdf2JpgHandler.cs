
using PdfiumRaster;

namespace YS.Knife.Document.Converter.Pdf2Image
{
    [AutoConstructor]
    [Service(typeof(IDocTypeConvertHandler))]
    public partial class Pdf2JpgHandler : DocTypeConvertHandlerBase<Pdf2JpgOption>
    {

        public override string Name => "pdf2jpg";

        public override StreamBody Convert(StreamBody input, Pdf2JpgOption options, CancellationToken token = default)
        {
            return PdfPageRenderer.Render(
                input,
                options.Dpi,
                options.Quality,
                PdfImageOutputFormat.Jpeg,
                "image/jpeg",
                "jpg",
                token);
        }
    }
}

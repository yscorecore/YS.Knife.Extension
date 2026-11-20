using YS.Knife.Document;
using YS.Knife.Hosting;

namespace YS.Knife.Document.Converter.Pdf2Image.IntegrationTest
{
    [AutoConstructor]
    public partial class Pdf2PngHandlerTest : KnifeHost
    {
        private Xunit.Abstractions.ITestOutputHelper _output;

        private IDocTypeConvertHandler GetHandler(string name)
        {
            var handlers = this.GetService<IEnumerable<IDocTypeConvertHandler>>();
            return handlers.First(h => string.Equals(h.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void ShouldResolveHandler()
        {
            var handler = GetHandler("pdf2png");
            handler.Should().NotBeNull();
            handler.Name.Should().Be("pdf2png");
        }

        [Fact]
        public void ShouldConvertSinglePagePdfToPng()
        {
            var handler = GetHandler("pdf2png");
            var pdfBytes = TestPdfFactory.CreateMinimalPdf(pageCount: 1);
            var input = StreamBody.FromBytes(pdfBytes, "application/pdf", "sample.pdf");

            using var result = handler.Convert(input, new Pdf2PngOption { Dpi = 72 });

            result.Should().NotBeNull();
            result.ContentType.Should().Be("image/png");
            result.FileName.Should().EndWith(".png");
            result.Length.Should().BeGreaterThan(0);
            result.Stream.Length.Should().BeGreaterThan(0);
            _output.WriteLine($"PNG output: {result.Length} bytes, file: {result.FileName}");
        }

        [Fact]
        public void ShouldConvertMultiPagePdfToSingleLongImage()
        {
            var handler = GetHandler("pdf2png");
            var pdfBytes = TestPdfFactory.CreateMinimalPdf(pageCount: 3);
            var input = StreamBody.FromBytes(pdfBytes, "application/pdf", "multi.pdf");

            using var result = handler.Convert(input, new Pdf2PngOption { Dpi = 72 });

            result.Should().NotBeNull();
            result.ContentType.Should().Be("image/png");
            result.FileName.Should().EndWith(".png");

            // The long image should be larger than a single page rendering.
            var singleBytes = TestPdfFactory.CreateMinimalPdf(pageCount: 1);
            var singleInput = StreamBody.FromBytes(singleBytes, "application/pdf", "single.pdf");
            using var singleResult = handler.Convert(singleInput, new Pdf2PngOption { Dpi = 72 });

            result.Stream.Length.Should().BeGreaterThan(singleResult.Stream.Length,
                "the stacked image should contain all pages and therefore be larger than a single page");
            _output.WriteLine($"Long image: {result.Stream.Length} bytes, single page: {singleResult.Stream.Length} bytes, file: {result.FileName}");
        }
    }
}

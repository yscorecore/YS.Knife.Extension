using YS.Knife.Document;
using YS.Knife.Hosting;

namespace YS.Knife.Document.Converter.Pdf2Image.IntegrationTest
{
    [AutoConstructor]
    public partial class Pdf2JpgHandlerTest : KnifeHost
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
            var handler = GetHandler("pdf2jpg");
            handler.Should().NotBeNull();
            handler.Name.Should().Be("pdf2jpg");
        }

        [Fact]
        public void ShouldConvertSinglePagePdfToJpg()
        {
            var handler = GetHandler("pdf2jpg");
            var pdfBytes = TestPdfFactory.CreateMinimalPdf(pageCount: 1);
            var input = StreamBody.FromBytes(pdfBytes, "application/pdf", "sample.pdf");

            using var result = handler.Convert(input, new Pdf2JpgOption { Dpi = 72, Quality = 80 });

            result.Should().NotBeNull();
            result.ContentType.Should().Be("image/jpeg");
            result.FileName.Should().EndWith(".jpg");
            result.Length.Should().BeGreaterThan(0);
            result.Stream.Length.Should().BeGreaterThan(0);
            _output.WriteLine($"JPG output: {result.Length} bytes, file: {result.FileName}");
        }

        [Fact]
        public void ShouldConvertMultiPagePdfToSingleLongImage()
        {
            var handler = GetHandler("pdf2jpg");
            var pdfBytes = TestPdfFactory.CreateMinimalPdf(pageCount: 3);
            var input = StreamBody.FromBytes(pdfBytes, "application/pdf", "multi.pdf");

            using var result = handler.Convert(input, new Pdf2JpgOption { Dpi = 72, Quality = 80 });

            result.Should().NotBeNull();
            result.ContentType.Should().Be("image/jpeg");
            result.FileName.Should().EndWith(".jpg");

            var singleBytes = TestPdfFactory.CreateMinimalPdf(pageCount: 1);
            var singleInput = StreamBody.FromBytes(singleBytes, "application/pdf", "single.pdf");
            using var singleResult = handler.Convert(singleInput, new Pdf2JpgOption { Dpi = 72, Quality = 80 });

            result.Stream.Length.Should().BeGreaterThan(singleResult.Stream.Length,
                "the stacked image should contain all pages and therefore be larger than a single page");
            _output.WriteLine($"Long image: {result.Stream.Length} bytes, single page: {singleResult.Stream.Length} bytes, file: {result.FileName}");
        }
    }
}

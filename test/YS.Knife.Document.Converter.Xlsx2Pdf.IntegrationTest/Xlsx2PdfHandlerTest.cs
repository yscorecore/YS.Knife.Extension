using YS.Knife.Document;
using YS.Knife.Hosting;

namespace YS.Knife.Document.Converter.Xlsx2Pdf.IntegrationTest
{
    /// <summary>
    /// End-to-end tests that resolve the <c>xlsx2pdf</c> handler through DI and convert a real
    /// spreadsheet to PDF via LibreOffice. These require LibreOffice to be installed; when it is
    /// absent the tests are skipped rather than failed.
    /// </summary>
    public class Xlsx2PdfHandlerTest : KnifeHost
    {
        private readonly Xunit.Abstractions.ITestOutputHelper _output;

        public Xlsx2PdfHandlerTest(Xunit.Abstractions.ITestOutputHelper output)
        {
            _output = output;
        }

        private IDocTypeConvertHandler GetHandler()
        {
            var handlers = this.GetService<IEnumerable<IDocTypeConvertHandler>>();
            return handlers.First(h => string.Equals(h.Name, "xlsx2pdf", StringComparison.OrdinalIgnoreCase));
        }

        private static bool SkipWhenLibreOfficeMissing()
        {
            return !LibreOfficeEngine.IsLibreOfficeInstalled();
        }

        [Fact]
        public void ShouldResolveHandler()
        {
            var handler = GetHandler();
            handler.Should().NotBeNull();
            handler.Name.Should().Be("xlsx2pdf");
        }

        [Fact]
        public void ShouldConvertXlsxToPdf()
        {
            if (SkipWhenLibreOfficeMissing())
            {
                _output.WriteLine("LibreOffice is not installed; skipping conversion test.");
                return;
            }

            var handler = GetHandler();
            var xlsxBytes = TestXlsxFactory.CreateXlsx();
            var input = StreamBody.FromBytes(xlsxBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "report.xlsx");

            using var result = handler.Convert(input, new Xlsx2PdfOption { SinglePageSheets = true });

            result.Should().NotBeNull();
            result.ContentType.Should().Be(Xlsx2PdfHandler.PdfContentType);
            result.FileName.Should().EndWith(".pdf");
            result.Length.Should().BeGreaterThan(0);
            result.Stream.Length.Should().BeGreaterThan(0);

            // A real PDF always starts with the "%PDF-" magic bytes.
            var header = new byte[5];
            result.Stream.Read(header, 0, header.Length);
            System.Text.Encoding.ASCII.GetString(header).Should().Be("%PDF-");

            _output.WriteLine($"PDF output: {result.Length} bytes, file: {result.FileName}");
        }

        [Fact]
        public void ShouldHonourSinglePageSheetsOption()
        {
            if (SkipWhenLibreOfficeMissing())
            {
                _output.WriteLine("LibreOffice is not installed; skipping conversion test.");
                return;
            }

            var handler = GetHandler();
            var xlsxBytes = TestXlsxFactory.CreateXlsx();
            var input = StreamBody.FromBytes(xlsxBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "report.xlsx");

            using var single = handler.Convert(input, new Xlsx2PdfOption { SinglePageSheets = true });

            var input2 = StreamBody.FromBytes(xlsxBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "report.xlsx");
            using var split = handler.Convert(input2, new Xlsx2PdfOption { SinglePageSheets = false });

            single.Should().NotBeNull();
            split.Should().NotBeNull();
            single.Length.Should().BeGreaterThan(0);
            split.Length.Should().BeGreaterThan(0);

            _output.WriteLine($"Single-page-sheets PDF: {single.Length} bytes; split-sheets PDF: {split.Length} bytes");
        }

        [Fact]
        public void ShouldRejectUnsupportedFormat()
        {
            var handler = GetHandler();
            var input = StreamBody.FromBytes(new byte[] { 1, 2, 3 }, "text/plain", "notes.txt");

            Action act = () => handler.Convert(input, new Xlsx2PdfOption());

            act.Should().Throw<NotSupportedException>();
        }
    }
}

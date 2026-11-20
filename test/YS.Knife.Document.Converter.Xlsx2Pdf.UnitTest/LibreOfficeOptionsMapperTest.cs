namespace YS.Knife.Document.Converter.Xlsx2Pdf.UnitTest
{
    /// <summary>
    /// Pure unit tests for <see cref="LibreOfficeOptionsMapper"/>. The mapper is a deterministic
    /// translation from <see cref="Xlsx2PdfOption"/> to LibreOfficeKit's <see cref="PdfOptions"/>,
    /// so it can be exercised without a LibreOffice installation.
    /// </summary>
    public class LibreOfficeOptionsMapperTest
    {
        [Fact]
        public void Map_NullOptions_UsesDefaults()
        {
            var result = LibreOfficeOptionsMapper.Map(null);

            result.Quality.Should().Be(LibreOfficeOptionsMapper.DefaultQuality);
            result.MaxImageResolutionDpi.Should().Be(LibreOfficeOptionsMapper.DefaultMaxImageResolutionDpi);
            result.SinglePageSheets.Should().BeTrue("the default option should keep sheets on single pages");
            result.ExportBookmarks.Should().BeTrue();
            result.UseTaggedPdf.Should().BeFalse();
            result.PageRange.Should().BeNull();
            result.PdfACompliance.Should().Be(LibreOfficeKit.Enums.PdfACompliance.None);
        }

        [Fact]
        public void Map_ForwardedValues_ArePreserved()
        {
            var options = new Xlsx2PdfOption
            {
                Quality = 75,
                MaxImageResolutionDpi = 150,
                ReduceImageResolution = false,
                SinglePageSheets = false,
                ExportBookmarks = false,
                UseTaggedPdf = true,
                ExportNotes = true,
                PageRange = "1-3",
                PdfACompliance = PdfAComplianceLevel.Level2B,
            };

            var result = LibreOfficeOptionsMapper.Map(options);

            result.Quality.Should().Be(75);
            result.MaxImageResolutionDpi.Should().Be(150);
            result.ReduceImageResolution.Should().BeFalse();
            result.SinglePageSheets.Should().BeFalse();
            result.ExportBookmarks.Should().BeFalse();
            result.UseTaggedPdf.Should().BeTrue();
            result.ExportNotes.Should().BeTrue();
            result.PageRange.Should().Be("1-3");
            result.PdfACompliance.Should().Be(LibreOfficeKit.Enums.PdfACompliance.Level2B);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        [InlineData(101)]
        [InlineData(1000)]
        public void ClampQuality_OutOfRange_FallsBackToDefault(int quality)
        {
            LibreOfficeOptionsMapper.ClampQuality(quality).Should().Be(LibreOfficeOptionsMapper.DefaultQuality);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(50)]
        [InlineData(100)]
        public void ClampQuality_InRange_IsPreserved(int quality)
        {
            LibreOfficeOptionsMapper.ClampQuality(quality).Should().Be(quality);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void ClampDpi_NonPositive_FallsBackToDefault(int dpi)
        {
            LibreOfficeOptionsMapper.ClampDpi(dpi).Should().Be(LibreOfficeOptionsMapper.DefaultMaxImageResolutionDpi);
        }

        [Fact]
        public void ClampDpi_Above1200_IsCapped()
        {
            LibreOfficeOptionsMapper.ClampDpi(5000).Should().Be(1200);
        }

        [Theory]
        [InlineData(72)]
        [InlineData(300)]
        [InlineData(1200)]
        public void ClampDpi_ReasonableValue_IsPreserved(int dpi)
        {
            LibreOfficeOptionsMapper.ClampDpi(dpi).Should().Be(dpi);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void MapTimeout_NonPositive_FallsBackToDefault(int seconds)
        {
            LibreOfficeOptionsMapper.MapTimeout(seconds)
                .Should().Be(TimeSpan.FromSeconds(LibreOfficeOptionsMapper.DefaultTimeoutSeconds));
        }

        [Fact]
        public void MapTimeout_Positive_IsPreserved()
        {
            LibreOfficeOptionsMapper.MapTimeout(30).Should().Be(TimeSpan.FromSeconds(30));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Normalize_BlankPageRange_ReturnsNull(string? pageRange)
        {
            LibreOfficeOptionsMapper.Normalize(pageRange).Should().BeNull();
        }

        [Fact]
        public void Normalize_TrimsWhitespace()
        {
            LibreOfficeOptionsMapper.Normalize("  1-5  ").Should().Be("1-5");
        }

        [Fact]
        public void MapCompliance_Unset_ReturnsNone()
        {
            LibreOfficeOptionsMapper.MapCompliance(null).Should().Be(LibreOfficeKit.Enums.PdfACompliance.None);
        }

        [Theory]
        [InlineData(PdfAComplianceLevel.Level1B, LibreOfficeKit.Enums.PdfACompliance.Level1B)]
        [InlineData(PdfAComplianceLevel.Level2B, LibreOfficeKit.Enums.PdfACompliance.Level2B)]
        [InlineData(PdfAComplianceLevel.Level3B, LibreOfficeKit.Enums.PdfACompliance.Level3B)]
        [InlineData(PdfAComplianceLevel.None, LibreOfficeKit.Enums.PdfACompliance.None)]
        public void MapCompliance_Levels_AreMapped(PdfAComplianceLevel input, LibreOfficeKit.Enums.PdfACompliance expected)
        {
            LibreOfficeOptionsMapper.MapCompliance(input).Should().Be(expected);
        }

        [Theory]
        [InlineData("report.xlsx", true)]
        [InlineData("Report.XLSX", true)]
        [InlineData("macro.xlsm", true)]
        [InlineData("template.xltx", true)]
        [InlineData("legacy.xls", true)]
        [InlineData("sheet.ods", true)]
        [InlineData("data.csv", true)]
        [InlineData("document.docx", false)]
        [InlineData("image.pdf", false)]
        [InlineData("noextension", false)]
        [InlineData(null, false)]
        [InlineData("", false)]
        public void IsSupportedExtension_ClassifiesSpreadsheets(string? fileName, bool expected)
        {
            LibreOfficeOptionsMapper.IsSupportedExtension(fileName).Should().Be(expected);
        }

        [Theory]
        [InlineData("report.xlsx", "report.pdf")]
        [InlineData("path/to/macro.xlsm", "macro.pdf")]
        [InlineData("noextension", "noextension.pdf")]
        [InlineData(null, "document.pdf")]
        [InlineData("", "document.pdf")]
        [InlineData("   ", "document.pdf")]
        public void BuildOutputFileName_SwapsExtensionForPdf(string? input, string expected)
        {
            LibreOfficeOptionsMapper.BuildOutputFileName(input).Should().Be(expected);
        }
    }
}

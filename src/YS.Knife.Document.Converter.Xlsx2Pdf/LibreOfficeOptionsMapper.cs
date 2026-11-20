using LibreOfficeKit;
using LibreOfficeKit.Enums;

namespace YS.Knife.Document.Converter.Xlsx2Pdf
{
    /// <summary>
    /// Translates <see cref="Xlsx2PdfOption"/> into the LibreOfficeKit <see cref="PdfOptions"/> filter data.
    /// This is deliberately a pure function: no I/O, no LibreOffice process, so it can be unit tested
    /// on machines without LibreOffice installed.
    /// </summary>
    internal static class LibreOfficeOptionsMapper
    {
        internal const int DefaultQuality = 90;
        internal const int DefaultMaxImageResolutionDpi = 300;
        internal const int DefaultTimeoutSeconds = 120;

        /// <summary>
        /// Spreadsheet file extensions accepted as converter input.
        /// </summary>
        internal static readonly string[] SupportedExtensions =
        {
            ".xlsx",
            ".xlsm",
            ".xltx",
            ".xltm",
            ".xls",
            ".xlt",
            ".ods",
            ".csv",
        };

        /// <summary>
        /// Builds the PDF export options, normalizing out-of-range values rather than rejecting them,
        /// so a caller supplying sloppy configuration still gets a sensible PDF.
        /// </summary>
        internal static PdfOptions Map(Xlsx2PdfOption? options)
        {
            options ??= new Xlsx2PdfOption();

            return new PdfOptions
            {
                Quality = ClampQuality(options.Quality),
                MaxImageResolutionDpi = ClampDpi(options.MaxImageResolutionDpi),
                ReduceImageResolution = options.ReduceImageResolution,
                // Sheets are scaled to one page each only when the caller asks for it; the
                // upstream default is false, so we forward the value verbatim.
                SinglePageSheets = options.SinglePageSheets,
                ExportBookmarks = options.ExportBookmarks,
                UseTaggedPdf = options.UseTaggedPdf,
                ExportNotes = options.ExportNotes,
                PageRange = Normalize(options.PageRange),
                PdfACompliance = MapCompliance(options.PdfACompliance),
            };
        }

        /// <summary>
        /// Quality is only meaningful in the 1-100 range; anything else falls back to the default.
        /// </summary>
        internal static int ClampQuality(int quality)
        {
            return quality is >= 1 and <= 100 ? quality : DefaultQuality;
        }

        /// <summary>
        /// Image DPI must be positive. Values above 1200 are pulled back to 1200 because LibreOffice
        /// silently produces gigantic files beyond that.
        /// </summary>
        internal static int ClampDpi(int dpi)
        {
            if (dpi <= 0)
            {
                return DefaultMaxImageResolutionDpi;
            }

            return Math.Min(dpi, 1200);
        }

        /// <summary>
        /// Returns a positive timeout, falling back to the default for non-positive input.
        /// </summary>
        internal static TimeSpan MapTimeout(int timeoutSeconds)
        {
            var seconds = timeoutSeconds > 0 ? timeoutSeconds : DefaultTimeoutSeconds;
            return TimeSpan.FromSeconds(seconds);
        }

        /// <summary>
        /// Blank page ranges are meaningless to the filter, so they collapse to null (meaning "all pages").
        /// </summary>
        internal static string? Normalize(string? pageRange)
        {
            return string.IsNullOrWhiteSpace(pageRange) ? null : pageRange.Trim();
        }

        /// <summary>
        /// Maps the local compliance enum onto LibreOfficeKit's enum, collapsing unset values to null.
        /// </summary>
        internal static PdfACompliance MapCompliance(PdfAComplianceLevel? level)
        {
            if (!level.HasValue)
            {
                return PdfACompliance.None;
            }

            return level.Value switch
            {
                PdfAComplianceLevel.Level1B => LibreOfficeKit.Enums.PdfACompliance.Level1B,
                PdfAComplianceLevel.Level2B => LibreOfficeKit.Enums.PdfACompliance.Level2B,
                PdfAComplianceLevel.Level3B => LibreOfficeKit.Enums.PdfACompliance.Level3B,
                _ => PdfACompliance.None,
            };
        }

        /// <summary>
        /// Returns true when the file name carries an extension LibreOffice can import as a spreadsheet.
        /// </summary>
        internal static bool IsSupportedExtension(string? fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return false;
            }

            var ext = Path.GetExtension(fileName);
            if (string.IsNullOrEmpty(ext))
            {
                return false;
            }

            return SupportedExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Builds the output file name by swapping the input extension for ".pdf".
        /// Falls back to "document.pdf" when the input name has no usable base name.
        /// </summary>
        internal static string BuildOutputFileName(string? inputFileName)
        {
            var baseName = string.IsNullOrWhiteSpace(inputFileName)
                ? string.Empty
                : Path.GetFileNameWithoutExtension(inputFileName);

            if (string.IsNullOrWhiteSpace(baseName))
            {
                baseName = "document";
            }

            return $"{baseName}.pdf";
        }
    }
}

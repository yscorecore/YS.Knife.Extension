namespace YS.Knife.Document.Converter.Xlsx2Pdf
{
    /// <summary>
    /// Options controlling how a spreadsheet is rendered into a PDF by LibreOffice.
    /// Values left at their defaults produce a print-oriented PDF.
    /// </summary>
    public class Xlsx2PdfOption
    {
        /// <summary>
        /// JPEG compression quality for embedded images (1-100). Out-of-range values are clamped.
        /// </summary>
        public int Quality { get; set; } = 90;

        /// <summary>
        /// Maximum resolution (DPI) for embedded images. Values <= 0 fall back to 300.
        /// </summary>
        public int MaxImageResolutionDpi { get; set; } = 300;

        /// <summary>
        /// Whether images are downsampled to <see cref="MaxImageResolutionDpi"/>.
        /// </summary>
        public bool ReduceImageResolution { get; set; } = true;

        /// <summary>
        /// When true each worksheet is scaled onto a single PDF page instead of being split
        /// across several pages. This is usually what you want for spreadsheet exports.
        /// </summary>
        public bool SinglePageSheets { get; set; } = true;

        /// <summary>
        /// Whether sheet/bookmark outlines are exported into the PDF.
        /// </summary>
        public bool ExportBookmarks { get; set; } = true;

        /// <summary>
        /// Whether to produce a tagged (accessible) PDF.
        /// </summary>
        public bool UseTaggedPdf { get; set; }

        /// <summary>
        /// Whether cell comments/notes are exported.
        /// </summary>
        public bool ExportNotes { get; set; }

        /// <summary>
        /// Optional page range to export, for example "1-3". Null or blank exports every page.
        /// </summary>
        public string? PageRange { get; set; }

        /// <summary>
        /// Optional PDF/A compliance level (null keeps LibreOffice's default versioning).
        /// </summary>
        public PdfAComplianceLevel? PdfACompliance { get; set; }

        /// <summary>
        /// Per-conversion timeout in seconds. Values <= 0 fall back to 120 seconds.
        /// </summary>
        public int TimeoutSeconds { get; set; } = 120;
    }
}

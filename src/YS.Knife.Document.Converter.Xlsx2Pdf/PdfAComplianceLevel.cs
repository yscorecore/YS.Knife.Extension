namespace YS.Knife.Document.Converter.Xlsx2Pdf
{
    /// <summary>
    /// PDF/A compliance levels supported by the exporter. Kept as a project-local enum so the
    /// public option surface does not leak the underlying LibreOfficeKit types.
    /// </summary>
    public enum PdfAComplianceLevel
    {
        /// <summary>
        /// No PDF/A compliance requested.
        /// </summary>
        None = 0,

        /// <summary>
        /// PDF/A-1b - the most broadly compatible archival level.
        /// </summary>
        Level1B = 1,

        /// <summary>
        /// PDF/A-2b - supports JPEG2000 and layers.
        /// </summary>
        Level2B = 2,

        /// <summary>
        /// PDF/A-3b - supports embedded files and attachments.
        /// </summary>
        Level3B = 3,
    }
}

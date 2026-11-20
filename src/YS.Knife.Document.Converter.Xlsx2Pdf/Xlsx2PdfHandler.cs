namespace YS.Knife.Document.Converter.Xlsx2Pdf
{
    /// <summary>
    /// Converts spreadsheet documents (.xlsx and friends) into PDF using LibreOffice.
    /// Registered under the converter name <c>xlsx2pdf</c>.
    /// </summary>
    [Service(typeof(IDocTypeConvertHandler))]
    public partial class Xlsx2PdfHandler : DocTypeConvertHandlerBase<Xlsx2PdfOption>
    {
        /// <summary>
        /// MIME type of the produced document.
        /// </summary>
        public const string PdfContentType = "application/pdf";

        private readonly LibreOfficeEngine _engine;

        public override string Name => "xlsx2pdf";

        public Xlsx2PdfHandler(LibreOfficeEngine engine)
        {
            _engine = engine ?? throw new ArgumentNullException(nameof(engine));
        }

        public override StreamBody Convert(StreamBody input, Xlsx2PdfOption options, CancellationToken token = default)
        {
            if (input is null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (input.Stream is null)
            {
                throw new ArgumentException("The input document has no content stream.", nameof(input));
            }

            if (!LibreOfficeOptionsMapper.IsSupportedExtension(input.FileName))
            {
                throw new NotSupportedException(
                    $"'{input.FileName}' is not a supported spreadsheet format. Supported extensions: "
                    + $"{string.Join(", ", LibreOfficeOptionsMapper.SupportedExtensions)}.");
            }

            // The source stream is supplied by the caller, so rewind rather than assume it is at 0.
            if (input.Stream.CanSeek)
            {
                input.Stream.Seek(0, SeekOrigin.Begin);
            }

            var outputStream = new MemoryStream();
            try
            {
                _engine.Convert(input.Stream, outputStream, input.FileName, options, token);

                if (outputStream.Length == 0)
                {
                    throw new InvalidOperationException(
                        $"LibreOffice produced an empty PDF for '{input.FileName}'.");
                }

                outputStream.Position = 0;
                return StreamBody.FromStream(
                    outputStream,
                    PdfContentType,
                    LibreOfficeOptionsMapper.BuildOutputFileName(input.FileName),
                    outputStream.Length,
                    autoSeekBegin: false);
            }
            catch
            {
                outputStream.Dispose();
                throw;
            }
        }
    }
}

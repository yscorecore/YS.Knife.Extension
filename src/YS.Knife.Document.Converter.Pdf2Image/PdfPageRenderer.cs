using PdfiumRaster;

namespace YS.Knife.Document.Converter.Pdf2Image
{
    /// <summary>
    /// Renders PDF pages to raster images (PNG/JPEG) using PdfiumRaster (PDFium + SkiaSharp).
    /// Single page returns the image directly; multiple pages are stacked vertically into one long image.
    /// </summary>
    internal static class PdfPageRenderer
    {
        public static StreamBody Render(
            StreamBody input,
            int dpi,
            int quality,
            PdfImageOutputFormat format,
            string outputContentType,
            string outputExtension,
            CancellationToken token)
        {
            var tempPdfPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.pdf");
            try
            {
                using (var fs = File.Create(tempPdfPath))
                {
                    input.Stream.CopyTo(fs);
                }

                int pageCount = PdfImageConverter.GetPageCount(tempPdfPath, null);
                token.ThrowIfCancellationRequested();

                var conversionOptions = new PdfImageConversionOptions
                {
                    Format = format,
                    Render = new PdfPageRenderOptions
                    {
                        Dpi = dpi <= 0 ? 150 : dpi,
                        BackgroundColor = 0xFFFFFFFFu,
                        FillBackground = true,
                    },
                    Encoding = new PdfImageEncodingOptions
                    {
                        Quality = Math.Clamp(quality <= 0 ? 75 : quality, 1, 100),
                    },
                };

                var baseName = Path.GetFileNameWithoutExtension(input.FileName);

                // Render every page (page numbers are 1-based).
                var bitmaps = new List<PdfBitmap>(pageCount);
                for (int p = 1; p <= pageCount; p++)
                {
                    token.ThrowIfCancellationRequested();
                    bitmaps.Add(PdfImageConverter.RenderPageNumber(tempPdfPath, p, conversionOptions, null));
                }

                PdfBitmap finalBitmap;
                if (bitmaps.Count == 1)
                {
                    finalBitmap = bitmaps[0];
                }
                else
                {
                    finalBitmap = StackVertically(bitmaps);
                }

                var outStream = new MemoryStream();
                PdfImageConverter.SaveBitmap(finalBitmap, outStream, format, conversionOptions.Encoding);
                outStream.Position = 0;
                return StreamBody.FromStream(outStream, outputContentType, $"{baseName}.{outputExtension}", outStream.Length, autoSeekBegin: false);
            }
            finally
            {
                try { if (File.Exists(tempPdfPath)) File.Delete(tempPdfPath); } catch { /* ignore */ }
            }
        }

        /// <summary>
        /// Stacks the given BGRA bitmaps vertically into a single bitmap whose width equals the
        /// widest source page. Narrower pages are left-aligned; remaining columns are filled with white.
        /// </summary>
        internal static PdfBitmap StackVertically(IReadOnlyList<PdfBitmap> bitmaps)
        {
            int maxWidth = 0;
            int totalHeight = 0;
            for (int i = 0; i < bitmaps.Count; i++)
            {
                if (bitmaps[i].Width > maxWidth) maxWidth = bitmaps[i].Width;
                totalHeight += bitmaps[i].Height;
            }

            int combinedStride = maxWidth * 4; // BGRA = 4 bytes per pixel
            var combinedPixels = new byte[combinedStride * totalHeight];

            // Fill with opaque white so columns outside narrower pages are not transparent black.
            for (int i = 0; i < combinedPixels.Length; i += 4)
            {
                combinedPixels[i] = 0xFF;     // B
                combinedPixels[i + 1] = 0xFF; // G
                combinedPixels[i + 2] = 0xFF; // R
                combinedPixels[i + 3] = 0xFF; // A
            }

            int yOffset = 0;
            for (int bi = 0; bi < bitmaps.Count; bi++)
            {
                var bm = bitmaps[bi];
                int rowBytes = bm.Width * 4;
                for (int row = 0; row < bm.Height; row++)
                {
                    int srcOffset = row * bm.Stride;
                    int dstOffset = (yOffset + row) * combinedStride;
                    Buffer.BlockCopy(bm.Pixels, srcOffset, combinedPixels, dstOffset, rowBytes);
                }
                yOffset += bm.Height;
            }

            return new PdfBitmap(maxWidth, totalHeight, combinedStride, combinedPixels);
        }
    }
}

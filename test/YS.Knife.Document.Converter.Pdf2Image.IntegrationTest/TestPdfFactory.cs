using System.IO;
using System.Text;

namespace YS.Knife.Document.Converter.Pdf2Image.IntegrationTest
{
    /// <summary>
    /// Builds a minimal valid PDF at runtime so tests do not depend on a binary fixture file.
    /// The PDFs contain blank pages only - sufficient to exercise the PDFium raster path.
    /// </summary>
    internal static class TestPdfFactory
    {
        public static byte[] CreateMinimalPdf(int pageCount = 1)
        {
            var ms = new MemoryStream();
            var enc = new ASCIIEncoding();
            var offsets = new List<long>();

            void Write(string s)
            {
                var bytes = enc.GetBytes(s);
                ms.Write(bytes, 0, bytes.Length);
            }

            Write("%PDF-1.4\n");

            // Object 1: Catalog
            offsets.Add(ms.Position);
            Write("1 0 obj\n<</Type/Catalog/Pages 2 0 R>>\nendobj\n");

            // Object 2: Pages
            offsets.Add(ms.Position);
            var kids = string.Join(" ", Enumerable.Range(0, pageCount).Select(i => $"{3 + i} 0 R"));
            Write($"2 0 obj\n<</Type/Pages/Kids[{kids}]/Count {pageCount}>>\nendobj\n");

            // Objects 3..3+pageCount-1: pages
            for (int i = 0; i < pageCount; i++)
            {
                offsets.Add(ms.Position);
                Write($"{3 + i} 0 obj\n<</Type/Page/Parent 2 0 R/MediaBox[0 0 595 842]>>\nendobj\n");
            }

            // xref table
            long xrefPos = ms.Position;
            int totalObjs = 2 + pageCount + 1; // free(0) + catalog + pages + pageCount
            Write($"xref\n0 {totalObjs}\n");
            Write("0000000000 65535 f \n");
            foreach (var off in offsets)
            {
                Write($"{off:D10} 00000 n \n");
            }

            Write($"trailer\n<</Size {totalObjs}/Root 1 0 R>>\nstartxref\n{xrefPos}\n%%EOF\n");

            return ms.ToArray();
        }
    }
}

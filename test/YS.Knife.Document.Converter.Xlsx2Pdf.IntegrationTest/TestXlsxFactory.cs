using System.IO.Compression;

namespace YS.Knife.Document.Converter.Xlsx2Pdf.IntegrationTest
{
    /// <summary>
    /// Builds a minimal but valid .xlsx (OOXML package) at runtime so tests do not depend on a
    /// binary fixture file. LibreOffice only needs a well-formed workbook to import it.
    /// </summary>
    internal static class TestXlsxFactory
    {
        /// <summary>
        /// Creates an .xlsx with a single worksheet containing the given rows of text.
        /// </summary>
        public static byte[] CreateXlsx(string sheetName = "Sheet1", string[][]? rows = null)
        {
            rows ??= new[] { new[] { "Name", "Amount" }, new[] { "Alice", "100" }, new[] { "Bob", "200" } };

            using var ms = new MemoryStream();
            using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
            {
                AddEntry(zip, "[Content_Types].xml",
                    "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                    "<Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\">" +
                    "<Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/>" +
                    "<Default Extension=\"xml\" ContentType=\"application/xml\"/>" +
                    "<Override PartName=\"/xl/workbook.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml\"/>" +
                    "<Override PartName=\"/xl/worksheets/sheet1.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/>" +
                    "</Types>");

                AddEntry(zip, "_rels/.rels",
                    "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                    "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">" +
                    "<Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument\" Target=\"xl/workbook.xml\"/>" +
                    "</Relationships>");

                AddEntry(zip, "xl/workbook.xml",
                    "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                    "<workbook xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\" " +
                    "xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\">" +
                    "<sheets><sheet name=\"Sheet1\" sheetId=\"1\" r:id=\"rId1\"/></sheets>" +
                    "</workbook>");

                AddEntry(zip, "xl/_rels/workbook.xml.rels",
                    "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                    "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">" +
                    "<Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet1.xml\"/>" +
                    "</Relationships>");

                AddEntry(zip, "xl/worksheets/sheet1.xml", BuildSheetXml(rows));
            }

            return ms.ToArray();
        }

        private static string BuildSheetXml(string[][] rows)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append(
                "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                "<worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\">" +
                "<sheetData>");

            for (int r = 0; r < rows.Length; r++)
            {
                sb.Append("<row r=\"").Append(r + 1).Append("\">");
                for (int c = 0; c < rows[r].Length; c++)
                {
                    var value = System.Security.SecurityElement.Escape(rows[r][c]) ?? string.Empty;
                    sb.Append("<c r=\"").Append(ColumnName(c)).Append(r + 1).Append("\"><v>").Append(value).Append("</v></c>");
                }
                sb.Append("</row>");
            }

            sb.Append("</sheetData></worksheet>");

            return sb.ToString();
        }

        private static string ColumnName(int index)
        {
            // A, B, C, ... Z, AA, AB, ...
            var name = string.Empty;
            index++;
            while (index > 0)
            {
                var rem = (index - 1) % 26;
                name = (char)('A' + rem) + name;
                index = (index - rem - 1) / 26;
            }
            return name;
        }

        private static void AddEntry(ZipArchive zip, string entryName, string content)
        {
            var entry = zip.CreateEntry(entryName, CompressionLevel.Fastest);
            using var writer = new StreamWriter(entry.Open(), System.Text.Encoding.UTF8);
            writer.Write(content);
        }
    }
}

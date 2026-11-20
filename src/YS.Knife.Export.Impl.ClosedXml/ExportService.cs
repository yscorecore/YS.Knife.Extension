using ClosedXML.Excel;
using Microsoft.Extensions.DependencyInjection;

namespace YS.Knife.Export.Impl.ClosedXml
{
    [AutoConstructor]
    [Service(Lifetime = ServiceLifetime.Singleton)]
    public partial class ExportService : BaseExportService
    {
        protected override string GetDataFileName(Guid token)
        {
            return Path.ChangeExtension(base.GetDataFileName(token), ".xlsx");
        }
        protected override Task WriteData(string fileName, EntityMetadata metadata, int metadataIndex, List<Dictionary<string, object>> datas)
        {
            using var workbook = new XLWorkbook(fileName);
            var sheet = workbook.Worksheet(metadataIndex + 1);
            var lastRow = sheet.LastRowUsed().RowNumber();
            var cellValues = datas.Select(p => metadata.Columns.Select(t =>
             {
                 if (p.TryGetValue(t.Name, out var val))
                 {
                     return val;
                 }
                 return null;
             }).ToArray()).ToList();
            sheet.Row(lastRow + 1).Cell(1).InsertData(cellValues);
            workbook.Save();
            return Task.CompletedTask;
        }

        protected override Task WriteDataTitle(string fileName, EntityMetadata[] metadatas)
        {
            using var workbook = new XLWorkbook();
            foreach (var metadata in metadatas)
            {
                var worksheet = workbook.Worksheets.Add(metadata.DisplayName ?? metadata.Name);
                for (int i = 0; i < metadata.Columns.Count; i++)
                {
                    var column = metadata.Columns[i];
                    var cell = worksheet.Cell(1, i + 1);
                    cell.Value = column.DisplayName ?? column.Name;
                    if (column.Width.HasValue)
                    {
                        cell.WorksheetColumn().Width = column.Width.Value;
                    }
                }
            }
            workbook.SaveAs(fileName);
            return Task.CompletedTask;
        }
    }
}

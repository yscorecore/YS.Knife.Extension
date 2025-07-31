using ClosedXML.Excel;
using Microsoft.Extensions.DependencyInjection;
using YS.Knife.Import.Abstractions;
using YS.Knife.Import.Impl.Base;

namespace YS.Knife.Import.ClosedXml
{
    [AutoConstructor]
    [Service(Lifetime = ServiceLifetime.Singleton)]
    public partial class ImportService : BaseImportService
    {

        protected override Task<Dictionary<string, List<Dictionary<string, object>>>> ReadData(string filePath, EntityMetadata[] entityMetadata)
        {
            using var workbook = new XLWorkbook(filePath);
            var res = new Dictionary<string, List<Dictionary<string, object>>>();
            foreach (var entity in (entityMetadata ?? Array.Empty<EntityMetadata>()))
            {
                var sheet = FindWorkSheet(workbook, entity) ?? throw Errors.CannotFindSheet(entity.Name);
                res[entity.Name] = ReadSheetData(sheet, entity);
            }
            return Task.FromResult(res);

        }
        private IXLWorksheet FindWorkSheet(XLWorkbook book, EntityMetadata entityMetadata)
        {
            if (entityMetadata.Position != null)
            {
                return book.Worksheet(entityMetadata.Position.Value);
            }
            var names = new List<string>()
            {
                entityMetadata.Name
            };
            if (entityMetadata.Alias != null)
            {
                names.AddRange(entityMetadata.Alias.Where(p => !string.IsNullOrEmpty(p)));
            }
            return book.Worksheets.Where(p => names.Contains(p.Name)).FirstOrDefault();

        }
        private List<Dictionary<string, object>> ReadSheetData(IXLWorksheet worksheet, EntityMetadata metadata)
        {
            var list = new List<Dictionary<string, object>>();
            var dic = GetColumnMaps(worksheet, metadata, out var titleIndex);
            var dataRowIndex = titleIndex + 1;
            var maxRows = worksheet.LastRowUsed().RowNumber();
            while (true && dataRowIndex <= maxRows)
            {
                var row = worksheet.Row(dataRowIndex);
                if (row == null)
                {
                    break;
                }
                else
                {
                    list.Add(ReadRowData(row, dic));
                }
                dataRowIndex++;
            }

            return list;

        }
        private Dictionary<string, object> ReadRowData(IXLRow row, Dictionary<int, EntityColumnMetadata> columns)
        {
            var res = new Dictionary<string, object>();
            foreach (var (position, meta) in columns)
            {
                var cell = row.Cell(position);
                res[meta.Name] = GetCellValue(cell, meta);
            }
            return res;
        }
        private object GetCellValue(IXLCell cell, EntityColumnMetadata metadata)
        {
            return metadata.DataType switch
            {
                DataType.String => cell.IsEmpty() ? null : cell.GetString(),
                DataType.Boolean => cell.IsEmpty() ? null : cell.GetBoolean(),
                DataType.DateTime => cell.IsEmpty() ? null : cell.GetDateTime(),
                DataType.Number => cell.IsEmpty() ? null : cell.GetDouble(),
                _ => throw new InvalidDataException(),
            };
        }

        private Dictionary<int, EntityColumnMetadata> GetColumnMaps(IXLWorksheet worksheet, EntityMetadata metadata, out int titleRowIndex)
        {
            //TOTO 动态获取表头
            titleRowIndex = 1;
            var titleRow = worksheet.FirstRowUsed();
            titleRowIndex = titleRow.RowNumber();
            Dictionary<string, int> titles = new Dictionary<string, int>();
            for (int i = 1; i <= worksheet.ColumnCount(); i++)
            {
                var content = worksheet.Cell(titleRowIndex, i).GetString();
                if (string.IsNullOrEmpty(content))
                {
                    break;
                }
                else
                {
                    titles[content.Trim()] = i;
                }
            }

            Dictionary<int, EntityColumnMetadata> columns = metadata.Columns.Where(p => p.Position.HasValue)
                .ToDictionary(p => p.Position!.Value);

            foreach (var column in metadata.Columns.Where(p => p.Position == null))
            {
                var names = new List<string> { column.Name };
                if (column.Alias != null)
                {
                    names.AddRange(column.Alias.Where(p => !string.IsNullOrEmpty(p)));
                }
                var index = FindColumnIndexByNames(names);
                if (index >= 0)
                {
                    columns[index] = column;
                }
                else
                {
                    if (!column.Optional)
                    {
                        throw Errors.CannotFindColumn(column.Name);
                    }
                }
            }
            return columns;
            int FindColumnIndexByNames(List<string> names)
            {
                foreach (var name in names)
                {
                    if (titles.TryGetValue(name, out var position))
                    {
                        return position;
                    }
                }
                return -1;
            }
        }

        public async override Task<Dictionary<string, List<ColumnInfo>>> ReadColumn(Guid token)
        {
            var exportinfo = await GetExportInfo(token);
            var res = new Dictionary<string, List<ColumnInfo>>();
            using (var workbook = new XLWorkbook(exportinfo.FilePath))
            {
                var sheetPages = workbook.Worksheets;
                foreach (var worksheet in sheetPages)
                {
                    var headerRow = worksheet.FirstRowUsed(); // 获取第一行（列头）
                    var col = new List<ColumnInfo>();
                    foreach (var cell in headerRow.Cells())
                    {
                        if (string.IsNullOrWhiteSpace(cell.GetString()))
                        {
                            continue;
                        }
                        var column = new ColumnInfo();
                        column.ColumnName = cell.GetString();
                        col.Add(column);
                    }
                    res.Add(worksheet.Name, col);
                }

            }
            return res;
        }
    }
}

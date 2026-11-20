using Microsoft.Extensions.DependencyInjection;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using YS.Knife.Import.Abstractions;
using YS.Knife.Import.Impl.Base;

namespace YS.Knife.Import.Impl.NPOI
{
    [AutoConstructor]
    [Service(Lifetime = ServiceLifetime.Singleton)]
    public partial class ImportService : BaseImportService
    {
        protected override Task<Dictionary<string, List<Dictionary<string, object>>>> ReadData(string filePath, EntityMetadata[] entityMetadata)
        {
            using var stream = File.Open(filePath, FileMode.OpenOrCreate);
            using var workbook = CreateWorkBookByName(stream, filePath);
            var res = new Dictionary<string, List<Dictionary<string, object>>>();
            foreach (var entity in (entityMetadata ?? Array.Empty<EntityMetadata>()))
            {
                var sheet = FindWorkSheet(workbook, entity) ?? throw Errors.CannotFindSheet(entity.Name);
                res[entity.Name] = ReadSheetData(sheet, entity);
            }
            return Task.FromResult(res);
        }
        private IWorkbook CreateWorkBookByName(Stream stream, string filePath)
        {
            if (Path.GetExtension(filePath).Equals(".xls", StringComparison.OrdinalIgnoreCase))
            {
                return new HSSFWorkbook(stream);
            }
            else if (Path.GetExtension(filePath).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                return new XSSFWorkbook(stream);
            }
            else
            {
                throw Errors.NotSupportFileFormat();
            }
        }
        private ISheet FindWorkSheet(IWorkbook book, EntityMetadata entityMetadata)
        {
            if (entityMetadata.Position != null)
            {
                return book.GetSheetAt(entityMetadata.Position.Value - 1);
            }
            var names = new List<string>()
            {
                entityMetadata.Name
            };
            if (entityMetadata.Alias != null)
            {
                names.AddRange(entityMetadata.Alias.Where(p => !string.IsNullOrEmpty(p)));
            }
            for (var i = 0; i < book.NumberOfSheets; i++)
            {
                if (names.Contains(book.GetSheetName(i)))
                {
                    return book.GetSheetAt(i);
                }
            }
            throw new Exception("找不到Sheet页");

        }
        private List<Dictionary<string, object>> ReadSheetData(ISheet worksheet, EntityMetadata metadata)
        {
            var list = new List<Dictionary<string, object>>();
            var dic = GetColumnMaps(worksheet, metadata, out var titleIndex);
            var dataRowIndex = titleIndex + 1;

            var maxRows = worksheet.LastRowNum;
            while (true && dataRowIndex <= maxRows)
            {
                var row = worksheet.GetRow(dataRowIndex);
                if (row == null)
                {
                    break;
                }
                else
                {
                    var rowData = ReadRowData(row, dic);
                    if (rowData.All(p => p.Value == null))
                    {
                        break;
                    }
                    else
                    {
                        list.Add(ReadRowData(row, dic));
                    }
                }
                dataRowIndex++;
            }

            return list;

        }
        private Dictionary<string, object> ReadRowData(IRow row, Dictionary<int, EntityColumnMetadata> columns)
        {
            var res = new Dictionary<string, object>();
            foreach (var (position, meta) in columns)
            {
                var cell = row.GetCell(position);
                res[meta.Name] = GetCellValue(cell, meta);
            }
            return res;
        }
        private object GetCellValue(ICell cell, EntityColumnMetadata metadata)
        {
            if (cell == null || cell.CellType == CellType.Blank) return null;
            try
            {
                return metadata.DataType switch
                {
                    DataType.String => Convert.ChangeType(GetCellValue(cell), typeof(string)),
                    DataType.Boolean => Convert.ChangeType(GetCellValue(cell), typeof(bool)),
                    DataType.DateTime => cell.DateCellValue,
                    DataType.Number => cell.NumericCellValue,
                    _ => throw new InvalidDataException(),
                };
            }
            catch (Exception ex)
            {

                throw new Exception($"在表格【{cell.Sheet.SheetName}】中读取单元格【{cell.Address}】的值出错", ex);
            }

        }
        private object GetCellValue(ICell cell)
        {
            return cell.CellType switch
            {
                CellType.Boolean => cell.BooleanCellValue,
                CellType.String => cell.StringCellValue,
                CellType.Numeric => cell.NumericCellValue,
                _ => cell.StringCellValue,
            };

        }

        private Dictionary<int, EntityColumnMetadata> GetColumnMaps(ISheet worksheet, EntityMetadata metadata, out int titleRowIndex)
        {
            //TOTO 动态获取表头
            titleRowIndex = 0;
            var titleRow = worksheet.GetRow(0);
            Dictionary<string, int> titles = new Dictionary<string, int>();
            for (int i = 0; i < titleRow.PhysicalNumberOfCells; i++)
            {
                var content = titleRow.GetCell(i).StringCellValue;
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
                .ToDictionary(p => p.Position!.Value - 1);

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

        public override async Task<Dictionary<string, List<ColumnInfo>>> ReadColumns(Guid token)
        {
            var res = new Dictionary<string, List<ColumnInfo>>();
            var exportinfo = await GetExportInfo(token);
            using (var stream = new FileStream(exportinfo.FilePath, FileMode.Open, FileAccess.Read))
            {
                using var workbook = CreateWorkBookByName(stream, exportinfo.FilePath);
                var sheetPages = workbook.NumberOfSheets;
                for (int i = 0; i < sheetPages; i++)
                {
                    var sheet = workbook.GetSheetAt(i);
                    IRow headerRow = sheet.GetRow(0);
                    var col = new List<ColumnInfo>();
                    if (headerRow == null)
                    {
                        continue;
                    }
                    for (int j = 0; j < headerRow.LastCellNum; j++)
                    {
                        ICell cell = headerRow.GetCell(j);
                        if (cell != null && !string.IsNullOrWhiteSpace(cell.ToString()))
                        {
                            var column = new ColumnInfo();
                            column.ColumnName = cell.ToString();
                            col.Add(column);
                        }
                    }
                    res.Add(sheet.SheetName, col);
                }
            }
            return res;
        }
    }
}

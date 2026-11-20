using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace YS.Knife.Export.Impl.NPOI
{
    [AutoConstructor]
    [Service(Lifetime = ServiceLifetime.Singleton)]
    public partial class ExportService : BaseExportService
    {
        private readonly ILogger<ExportService> logger;
        protected override string GetDataFileName(Guid token)
        {
            return Path.ChangeExtension(base.GetDataFileName(token), ".xlsx");
        }
        protected override Task WriteData(string fileName, EntityMetadata metadata, int metadataIndex, List<Dictionary<string, object>> datas)
        {
            IWorkbook workbook;
            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                workbook = new XSSFWorkbook(fs);
            }

            ISheet sheet = workbook.GetSheetAt(metadataIndex);
            var columns = metadata.Columns.Select((p, i) => new { Index = i, Column = p }).ToList();
            foreach (var data in datas)
            {
                IRow newRow = sheet.CreateRow(sheet.LastRowNum + 1);
                columns.ForEach(p =>
                {
                    // var cell = newRow.CreateCell()
                    if (data.TryGetValue(p.Column.Name, out var val))
                    {
                        var cell = newRow.CreateCell(p.Index);
                        SetCellValue(cell, val, p.Column);
                    }
                });
            }
            // 将修改写回文件
            using (FileStream fos = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                workbook.Write(fos);
            }
            return Task.CompletedTask;

            void SetCellValue(ICell cell, object value, EntityColumnMetadata columnInfo)
            {
                try
                {
                    switch (columnInfo.DataType)
                    {
                        case DataType.Boolean:
                            cell.SetCellValue(Convert.ToBoolean(value));
                            break;
                        case DataType.Number:
                            cell.SetCellValue(Convert.ToDouble(value));
                            break;
                        case DataType.DateTime:
                            cell.SetCellValue(Convert.ToDateTime(value));
                            break;
                        default:
                            cell.SetCellValue(Convert.ToString(value));
                            break;
                    }
                }
                catch (Exception ex)
                {
                    cell.SetCellValue(ex.Message);
                    logger.LogWarning(ex, "Error when set cell value {value} as {type}.", value, columnInfo.DataType);
                }

            }

        }


        protected override Task WriteDataTitle(string fileName, EntityMetadata[] metadatas)
        {
            using var workbook = new XSSFWorkbook();
            foreach (var metadata in metadatas)
            {
                var worksheet = workbook.CreateSheet(metadata.DisplayName ?? metadata.Name);
                var defaultCellStyle = workbook.CreateCellStyle();

                var row = worksheet.CreateRow(0);
                var style = workbook.CreateCellStyle();
                style.FillPattern = FillPattern.SolidForeground; // 设置填充模式为纯色填充
                style.FillForegroundColor = IndexedColors.LightBlue.Index;
                for (int i = 0; i < metadata.Columns.Count; i++)
                {
                    var column = metadata.Columns[i];
                    var cell = row.CreateCell(i);
                    cell.SetCellValue(column.DisplayName ?? column.Name);
                    cell.CellStyle = style;
                }
            }
            using var stream = File.OpenWrite(fileName);
            workbook.Write(stream, false);
            logger.LogInformation("Create data file {fileName}.", fileName);
            return Task.CompletedTask;
        }

    }
}

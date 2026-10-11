# YS.Knife.Import.Impl.NPOI

基于 NPOI 的 Excel 导入服务实现，继承 `BaseImportService`，同时支持 `.xls`（HSSF）与 `.xlsx`（XSSF）格式，以 Singleton 生命周期自动注册为 `IImportService`。

## 主要功能

- 按文件扩展名自动选择工作簿引擎（`.xls` → `HSSFWorkbook`，`.xlsx` → `XSSFWorkbook`，其他格式抛 `NotSupportFileFormat`）
- 按 `EntityMetadata` 定位工作表（`Position` 从 1 开始，或 `Name`/`Alias` 匹配）
- 以第一行为表头，按列名/别名/位置映射到 `EntityColumnMetadata`
- 按 `DataType` 读取单元格值：`String` / `Boolean` / `DateTime` / `Number`，读取失败时附带工作表名与单元格地址的错误信息
- 整行数据均为空时自动终止读取
- `ReadColumns` 返回每个工作表的列头列表

## 快速开始

1. 引用本包（自动带入 `Import.Impl.Base` 与 `Import.Core`），并确保注册了 `IDistributedCache`。

2. 可选配置：

```json
{
  "Import": {
    "DataFolder": "import_data",
    "ExpiredIn": 7200
  }
}
```

3. 注入使用：

```csharp
public class DemoService
{
    private readonly IImportService importService;

    public DemoService(IImportService importService) => this.importService = importService;

    public async Task ImportAsync(Stream stream, string fileName)
    {
        var token = await importService.BeginImport(stream, fileName);

        var data = await importService.ReadData(token.Token, new[]
        {
            new EntityMetadata
            {
                Name = "Sheet1",
                Alias = new[] { "用户" },
                Columns = new List<EntityColumnMetadata>
                {
                    new() { Name = "姓名", DataType = DataType.String },
                    new() { Name = "年龄", DataType = DataType.Number, Optional = true },
                }
            }
        });

        await importService.EndImport(token.Token);
    }
}
```

## 依赖

- `NPOI`（2.6.2）：Excel 解析（xls/xlsx）
- `YS.Knife.Import.Impl.Base`（项目引用）：导入流程基类与配置
- 如需 HTTP 接口，搭配 `YS.Knife.Import.Api.AspnetCore`

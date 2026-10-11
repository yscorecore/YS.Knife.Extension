# YS.Knife.Import.Impl.ClosedXml

基于 ClosedXML 的 Excel（xlsx）导入服务实现，继承 `BaseImportService`，以 Singleton 生命周期自动注册为 `IImportService`。

## 主要功能

- 解析 `.xlsx` 文件（`XLWorkbook`），按 `EntityMetadata` 定位工作表（支持 `Position` 或 `Name`/`Alias` 匹配）
- 自动识别首行表头，按列名/别名/位置将列映射到 `EntityColumnMetadata`
- 按声明的 `DataType` 读取单元格值：`String` / `Boolean` / `DateTime` / `Number`
- 列缺失时：非 `Optional` 列抛出 `CannotFindColumn` 异常，可选列跳过
- `ReadColumns` 返回每个工作表的列头列表
- 找不到工作表时抛出 `CannotFindSheet` 异常

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
        var columns = await importService.ReadColumns(token.Token);

        var data = await importService.ReadData(token.Token, new[]
        {
            new EntityMetadata
            {
                Name = "Sheet1",
                Columns = new List<EntityColumnMetadata>
                {
                    new() { Name = "姓名", DataType = DataType.String },
                    new() { Name = "入职日期", DataType = DataType.DateTime, Optional = true },
                }
            }
        });

        await importService.EndImport(token.Token);
    }
}
```

## 依赖

- `ClosedXML`（0.102.1）：Excel 解析
- `YS.Knife.Import.Impl.Base`（项目引用）：导入流程基类与配置
- 如需 HTTP 接口，搭配 `YS.Knife.Import.Api.AspnetCore`

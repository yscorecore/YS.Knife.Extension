# YS.Knife.Import.Api.AspnetCore

数据导入功能的 ASP.NET Core Web API 层，基于 `IImportService` 提供文件上传导入、列头读取、数据读取、结束导入的 HTTP 接口。

## 主要功能

内置 `ImportController`（路由前缀 `import`）：

| 路由 | 方法 | 说明 |
| --- | --- | --- |
| `POST import/begin` | `BeginImport` | 上传文件（`IFormFile`），开始导入并返回 `ImportToken` |
| `GET import/read-column` | `ReadColumns` | 根据 token 读取各工作表的列头信息 |
| `POST import/read-data` | `ReadData` | 按提交的 `EntityMetadata[]` 元数据读取表格数据 |
| `GET import/complete` | `EndImport` | 结束导入，清理临时文件与缓存 |

## 快速开始

引用本包后 Controller 自动进入 MVC 管线，同时需注册一个 `IImportService` 实现（如 `YS.Knife.Import.Impl.ClosedXml` 或 `YS.Knife.Import.Impl.NPOI`）。

典型导入流程：

```csharp
// 1. 前端上传文件 → POST /import/begin
//    响应：{ "token": "...", "expiredIn": 7200 }

// 2. 读取列头 → GET /import/read-column?token=...

// 3. 按映射元数据读取数据 → POST /import/read-data?token=...
var metadata = new[]
{
    new EntityMetadata
    {
        Name = "用户表",
        Columns = new List<EntityColumnMetadata>
        {
            new() { Name = "姓名", DataType = DataType.String },
            new() { Name = "年龄", DataType = DataType.Number, Optional = true },
        }
    }
};

// 4. 完成后 → GET /import/complete?token=...
```

## 依赖

- `YS.Knife.Import.Core`（项目引用）：`IImportService` 抽象及 DTO 定义
- 目标框架 `Microsoft.NET.Sdk.Web`，面向 ASP.NET Core
- 需要搭配一个 `*.Impl.*` 实现包（ClosedXml 或 NPOI）才能实际工作

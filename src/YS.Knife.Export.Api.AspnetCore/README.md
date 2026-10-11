# YS.Knife.Export.Api.AspnetCore

大数据量分步导出的 Web API 层，将 `IExportService` 的导出流程（开始 → 追加数据 → 完成下载 / 取消）暴露为 HTTP 接口。

## 接口一览

`ExportController`，路由前缀 `/export`：

| 方法 | 路由 | 说明 |
| --- | --- | --- |
| POST | `/export/begin` | 提交实体元数据（`EntityMetadata[]`），开始导出，返回 `ExportToken` |
| POST | `/export/append-data?token={token}` | 追加一批数据（`EntityData`），可多次调用 |
| GET | `/export/complete?token={token}&fileName={fileName}` | 结束导出并下载文件（Excel 格式响应） |
| GET | `/export/cancel?token={token}` | 取消导出并清理临时文件 |

## 调用流程示例

```csharp
// 1. 开始导出，声明列结构
var token = await http.PostAsJsonAsync("/export/begin", new[]
{
    new EntityMetadata
    {
        Name = "users",
        DisplayName = "用户列表",
        Columns = new List<EntityColumnMetadata>
        {
            new() { Name = "Name", DisplayName = "姓名" },
            new() { Name = "Age", DisplayName = "年龄", DataType = DataType.Number }
        }
    }
});

// 2. 分页多次追加数据
await http.PostAsJsonAsync($"/export/append-data?token={token}", new EntityData
{
    Name = "users",
    Datas = pageOfRows   // List<Dictionary<string, object>>
});

// 3. 完成并下载
// GET /export/complete?token=...&fileName=用户列表.xlsx

// 或取消
// GET /export/cancel?token=...
```

## 说明

- 本层只负责 HTTP 协议转换，全部业务逻辑委托给 `IExportService`（定义于 `YS.Knife.Export.Core`）。
- 需要在应用中注册 `IExportService` 的具体实现（例如基于 `YS.Knife.Export.Impl.Base` 的 `BaseExportService` 派生实现）。
- 命名规则：`*.Api.AspnetCore` 为 Web API 层，`*.Core` 为抽象，`*.Impl.Xxx` 为实现。

## 依赖

- 项目引用：`YS.Knife.Export.Core`
- `YS.Knife.Core`（`[AutoConstructor]` 源生成器，间接依赖）
- 目标框架：.NET 6+（`Microsoft.NET.Sdk.Web`）

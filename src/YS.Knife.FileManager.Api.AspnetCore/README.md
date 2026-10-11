# YS.Knife.FileManager.Api.AspnetCore

云文件管理的 ASP.NET Core Web API 层，对外暴露文件/目录的创建、删除、重命名接口。

## 主要功能

- 提供 `CloudFileController`（路由 `api/cloudfile`），调用 `ICloudFileManagerService` 完成业务操作
- 接口列表：
  - `POST api/cloudfile`：批量创建文件记录（`CreateFileDto<Guid>[]`），返回 `Guid[]`
  - `POST api/cloudfile/folders`：批量创建目录（`CreateFolderDto<Guid>[]`）
  - `DELETE api/cloudfile`：按 `Guid[]` 批量删除
  - `POST api/cloudfile/rename`：重命名（`RenameFileDto<Guid>`）

## 使用示例

引用本包后，控制器随 ASP.NET Core 的应用程序部件自动被发现；需确保已实现并注册 `ICloudFileManagerService`（可配合 `YS.Knife.FileManager.Impl.EFCore` 使用）。

客户端调用示例：

```csharp
// 创建文件
var ids = await client.PostAsObject<Guid[]>(
    baseUrl, "api/cloudfile",
    body: new[]
    {
        new { name = "readme.txt", key = "k1", url = "/files/k1", size = 1024, parentId = (Guid?)null }
    });

// 创建目录
await client.PostAsString(baseUrl, "api/cloudfile/folders",
    body: new[] { new { name = "文档", parentId = (Guid?)null } });

// 重命名
await client.PostAsString(baseUrl, "api/cloudfile/rename",
    body: new { id = ids[0], name = "新名字.txt" });

// 删除
await client.DeleteAsString(baseUrl, "api/cloudfile",
    body: ids);
```

## 依赖说明

- 项目引用 `YS.Knife.FileManager.Core`：`ICloudFileManagerService` 接口及 `CreateFileDto`、`CreateFolderDto`、`RenameFileDto` 等模型
- 使用 `Microsoft.NET.Sdk.Web`，目标框架 .NET 6.0

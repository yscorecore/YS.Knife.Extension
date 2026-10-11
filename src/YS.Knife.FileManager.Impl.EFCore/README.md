# YS.Knife.FileManager.Impl.EFCore

基于 EF Core 的云文件管理服务实现，实现 `ICloudFileService` 与 `ICloudFileManagerService` 接口。

## 主要功能

- `CloudFileService`：通过 `[Service]` 自动注册，提供文件/目录管理能力
  - `Create`：批量创建文件（重名校验、父级必须为目录、自动提取扩展名）
  - `CreateFolder`：批量创建目录（重名校验）
  - `Delete`：批量软删除
  - `Rename`：重命名
  - `QueryPagedList`：分页查询（目录优先、按名称排序）
- 多逻辑角色隔离：所有数据按 `Owner` 归属当前主逻辑角色（通过 `ILogicRoleProvider` 解析）
- 业务异常带编码（`[CodeExceptions]`，前缀 `CloudFileService`），如：文件已存在、父级不是目录、存在重复名称等
- 配置项 `CloudFileOptions`：
  - `MainLogicRoleProvider`：主逻辑角色提供者名称
  - `LogicRoleProviders`（必填）：逻辑角色提供者列表

## 使用示例

引用本包并通过 YS.Knife 服务扫描注册后，在配置中提供选项：

```json
{
  "CloudFileOptions": {
    "MainLogicRoleProvider": "default",
    "LogicRoleProviders": [ "default" ]
  }
}
```

业务代码中注入接口使用：

```csharp
public class MyService
{
    private readonly ICloudFileManagerService fileManager;

    public MyService(ICloudFileManagerService fileManager) => this.fileManager = fileManager;

    public async Task<Guid> CreateFolderAsync(string name)
    {
        var ids = await fileManager.CreateFolder(
            new[] { new CreateFolderDto<Guid> { Name = name } });
        return ids[0];
    }
}
```

可配合 `YS.Knife.FileManager.Api.AspnetCore` 直接暴露 Web API。

## 依赖说明

- 项目引用：`YS.Knife.FileManager.Core`（接口与 DTO）、`YS.Knife.FileManager.Entity.EFCore`（实体）、`YS.Knife.EFCore`、`YS.Knife.EntityBase`、`YS.Knife.LogicRoles.Core`
- `YS.Knife.Core`：`[AutoConstructor]`、`[Service]`、`[Mapper]`、`[Options]`、`[CodeExceptions]` 等源生成特性

目标框架：.NET 6.0。

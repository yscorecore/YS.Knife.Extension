# YS.Knife.Metadata.Api.AspnetCore

元数据服务的 ASP.NET Core Web API 层，将 `IMetadataService` 暴露为 HTTP 接口。

## 主要功能

- 提供 `MetadataController`（路由 `api/metadata`），转发调用 `IMetadataService`：
  - `GET api/metadata/{name}`：获取指定名称的元数据信息（`MetadataInfo`）
  - `GET api/metadata/list-all`：列出全部元数据名称
- 控制器使用 `[AutoConstructor]` 自动注入依赖，无需手写构造函数

## 快速开始

在 ASP.NET Core 项目中引用本包，并注册一个 `IMetadataService` 实现（如 `YS.Knife.Metadata.Impl.Mvc`），然后确保控制器被发现：

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddApplicationPart(typeof(MetadataController).Assembly);

// 注册 IMetadataService 实现
// builder.Services.AddMetadataMvc(...);

var app = builder.Build();
app.MapControllers();
app.Run();
```

请求示例：

```text
GET /api/metadata/list-all
=> ["User", "Order", ...]

GET /api/metadata/User
=> { "name": "User", "columns": [...], ... }
```

## 依赖说明

- `YS.Knife.Metadata.Core`：提供 `IMetadataService`、`MetadataInfo` 等抽象定义
- `FlyTiger`（通过 `YS.Knife.Core` 体系）：提供 `[AutoConstructor]` 源代码生成能力
- 目标框架：net6.0，`Microsoft.NET.Sdk.Web`（类库形式输出）

# YS.Knife.Generators.ExposeApi

C# 增量源代码生成器（Incremental Source Generator）：在编译期为标注了 `[ExposeApi]` 的类自动生成 ASP.NET Core Web API Controller，无需手写控制器即可把服务接口暴露为 HTTP API。

## 工作原理

- 生成器自动注入 `YS.Knife.ExposeApiAttribute` 定义到使用者项目
- 在任意类上标注 `[ExposeApi(typeof(IMyService))]`，编译时即为该服务类型生成对应的 `XxxController`（派生自 `ControllerBase`，带 `[ApiController]`）
- 根据服务方法的**名称约定**推断 HTTP 动词：

| 方法名前缀 | HTTP 动词 |
| --- | --- |
| `get/query/find/fetch/list` | HttpGet |
| `create/add/post/upload/save/write` | HttpPost |
| `update/modify/edit` | HttpPut |
| `delete/remove` | HttpDelete |
| `patch` | HttpPatch |
| 其他 | 有返回值 → Get，无返回值 → Post |

- 第一个参数名为 `id`/`name`/`key` 时自动作为路由参数（`{id}`）
- 支持 `Stream` / `IFormFile` 文件上传、`YS.Knife.StreamBody` 文件下载、多个 Body 参数自动合并为 record
- XML 注释自动透传到生成的 Controller，便于 Swagger 展示

## 快速开始

1. 在 ASP.NET Core 项目中以 Analyzer 方式引用该包：

```xml
<PackageReference Include="YS.Knife.Generators.ExposeApi" Version="*" PrivateAssets="all" />
```

2. 标注 `[ExposeApi]`：

```csharp
using YS.Knife;

[ExposeApi(typeof(IUserService), RoutePrefix = "api/[controller]", AllowAnonymous = false)]
public class UserApiExpose
{
}
```

编译后自动生成：

```csharp
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService userService;
    // GET api/user/{id}  →  GetUserAsync([FromRoute] string id)
    // POST api/user      →  CreateUserAsync([FromBody] ...)
}
```

## ExposeApiAttribute 常用配置

| 属性 | 说明 | 默认值 |
| --- | --- | --- |
| `RoutePrefix` | Controller 路由前缀 | `api/[controller]` |
| `AllowAnonymous` | 是否生成 `[AllowAnonymous]` | `false` |
| `Includes` / `Excludes` | 只生成/排除指定方法名 | 空 |
| `HttpGetMatches` 等 | 自定义各动词的名称匹配正则 | 见上表 |
| `RouteParameterPattern` | 路由参数名匹配正则 | `^(id\|name\|key)$` |
| `TypeAttributePatterns` / `MethodAttributePatterns` / `ParameterAttributePatterns` | Attribute 透传规则，支持 `*`、`?` 通配符 | 参数默认透传 `System.ComponentModel.DataAnnotations.*` |
| `UseOperationIdAsRoute` | 使用 `YS.Knife.Operations.OperationAttribute` 的 Id 作为路由名 | `true` |

## 依赖

- `Microsoft.CodeAnalysis.CSharp`（4.4.0）/ `Microsoft.CodeAnalysis.Analyzers`
- 目标框架 `netstandard2.0`，作为编译期分析器运行，不参与运行时

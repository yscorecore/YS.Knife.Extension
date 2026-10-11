# YS.Knife.AspnetCore

ASP.NET Core 通用基础设施库，提供统一的 API 响应包装（CodeResult）、全局异常处理、泛型控制器支持以及 Swagger 辅助功能。

## 主要功能

- **统一响应包装**：`CodeResult` / `CodeResult<T>` 响应模型，配合 `WrapCodeResultAttribute` 过滤器自动将 Action 返回值包装为 `{ code, message, data }` 结构。
- **异常处理**：`WrapCodeResultAttribute` 同时实现 `IExceptionFilter`，将异常转换为 `CodeResult` 响应；支持带 `Code` 属性的自定义异常（通过反射缓存提取错误码）；`GlobalExceptionMiddleware` 提供全局 500 异常 JSON 响应。
- **Swagger 集成**：`CodeResultApplicationModelProvider` 自动为被包装的 Action 修正 `ProducesResponseType` 元数据；`CustomSwagger` 单例提供唯一的 SchemaId 与 OperationId 生成。
- **泛型控制器**：`GenericControllerAttribute` + `GenericControllerFeatureProvider` 支持基于泛型控制器自动生成多个封闭类型控制器（如数据字典 API）。

## 快速开始

### 包装 API 响应

```csharp
// 全局注册（对所有 Controller 生效）
builder.Services.AddControllers(options =>
{
    options.Filters.Add<WrapCodeResultAttribute>();
});

// 或仅对单个 Controller / Action 生效
[WrapCodeResult]
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    [HttpGet("{id}")]
    public UserDto GetUser(Guid id) => ...; // 实际返回 {"code":"0","message":"success","data":{...}}
}
```

返回值会被自动包装：

```json
{ "code": "0", "message": "success", "data": { "name": "张三" } }
```

在个别 Action 上跳过包装：

```csharp
[WrapCodeResultIgnore]
[HttpGet("raw")]
public IActionResult GetRaw() => Ok(...);
```

### 自定义业务异常

异常类只要包含公开的 `Code` 实例属性，抛出后会自动转换为对应错误码的 `CodeResult` 响应（HTTP 200）；`ValidationException` 返回 400；其他异常返回 500。

### 全局异常中间件

```csharp
app.UseMiddleware<GlobalExceptionMiddleware>();
```

### 泛型控制器

继承 `GenericControllerAttribute` 定义泛型控制器约定，并在 `AddControllers` 时加入 `GenericControllerFeatureProvider`：

```csharp
builder.Services.AddControllers()
    .ConfigureApplicationPartManager(m =>
        m.FeatureProviders.Add(new GenericControllerFeatureProvider()));
```

可参考 `YS.Knife.DataItem.Api.AspnetCore` 中的实际用法。

## 依赖

- `YS.Knife.Core`（提供 `[AutoConstructor]`、`[Service]` 等源生成器）
- 目标框架：.NET 6+（`Microsoft.NET.Sdk.Web`）

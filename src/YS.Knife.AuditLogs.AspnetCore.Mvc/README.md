# YS.Knife.AuditLogs.AspnetCore.Mvc

ASP.NET Core MVC 的审计日志集成层，通过 Action 过滤器自动记录控制器操作的执行日志，并交给注册的 `IAuditLogWriter` 写出。

## 主要功能

- `AuditLogAttribute`：Action 过滤器（`Order = -6000`），在 Action 执行前后自动创建审计日志上下文：
  - 默认只记录 `POST`、`PUT`、`DELETE`、`PATCH` 请求（可通过 `LogHttpMethods` 调整）。
  - 自动从方法的 `Operation` 元数据中解析 `OperationId` 与 `OperationDesc`。
  - 记录执行时间、耗时、返回值、异常信息。
  - 执行前后分别调用所有 `IAutitLogExecutingDataEnricher` / `IAutitLogExecutedDataEnricher` 丰富日志数据。
  - 最终调用所有已注册的 `IAuditLogWriter` 写出日志，单个 Writer 出错不影响其他 Writer。
- `NonAuditLogAttribute`：标记在方法或类上，跳过审计日志记录。
- `AuditLogAccessor` / `IAuditLogAccessor`：以 Scoped 生命周期提供当前请求的审计日志对象，可在业务代码中向 `Log.Datas` 追加自定义数据。
- `AuditLogKeys`：预定义的扩展数据键常量（`RequestIp`、`User`、`RequestUrl`、`RequestMethod`、`RequestObject`、`ResponseObject`）。
- `AuditLogOptions`：配置项，`IgnoreDataTypes` 指定日志中忽略的参数类型（默认忽略 `CancellationToken`、`Stream`、`IFormFile` 等）。

## 使用示例

```csharp
[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly IAuditLogAccessor auditLogAccessor;

    public UserController(IAuditLogAccessor auditLogAccessor)
    {
        this.auditLogAccessor = auditLogAccessor;
    }

    [HttpPost]
    [AuditLog] // 记录该操作的审计日志
    public IActionResult Create(CreateUserRequest request)
    {
        // 可追加自定义日志数据
        auditLogAccessor.Log.Datas["remark"] = "后台手动创建";
        return Ok();
    }

    [HttpPost("batch")]
    [AuditLog]
    [NonAuditLog] // 跳过记录
    public IActionResult BatchCreate() => Ok();
}
```

日志的具体写出由 `IAuditLogWriter` 的实现决定，可参考 `YS.Knife.AuditLogs.Writer.Default`。

## 依赖说明

- `YS.Knife.AuditLogs.Core`：审计日志核心抽象（`IAuditLog`、`IAuditLogWriter`、Enricher 接口等）。
- `YS.Knife.Operations.Core`：提供 `Operation` 元数据解析（`GetOperation()`）。
- `YS.Knife.Core`：服务自动注册（内部 `ServiceRegister` 注册 `IAuditLogAccessor` 并配置默认忽略的参数类型）。

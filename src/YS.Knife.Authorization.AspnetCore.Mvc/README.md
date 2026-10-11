# YS.Knife.Authorization.AspnetCore.Mvc

ASP.NET Core MVC 的功能权限（Function Permission）鉴权集成层，通过 `AuthAttribute` 过滤器基于操作（Operation）元数据进行权限校验。

## 主要功能

- `AuthAttribute`：异步授权过滤器，可标注在方法、类或程序集上，工作流程：
  1. 若前置过滤器已产生结果（`context.Result != null`），跳过校验。
  2. 若终结点标记了 `IAllowAnonymous`（如 `[AllowAnonymous]`），跳过校验。
  3. 仅对 Controller Action 生效；从方法的 `Operation` 元数据解析出 `AppId` 与操作 `Id`。
  4. 调用 `IFunctionPermissionService.HasPermission(appId, operationId)` 判断当前用户是否有权限；无权限时返回 `403 ForbidResult`。
- 校验过程输出详细的 `ILogger` 日志（通过 / 拒绝均可见用户与操作 ID）。

## 使用示例

```csharp
[ApiController]
[Route("api/orders")]
[Auth] // 该控制器下所有 Action 均进行功能权限校验
public class OrderController : ControllerBase
{
    [HttpPost]
    [Operation("order.create", "创建订单")] // 来自 YS.Knife.Operations
    public IActionResult Create() => Ok();

    [HttpGet("health")]
    [AllowAnonymous] // 匿名访问，跳过校验
    public IActionResult Health() => Ok();
}
```

需要配合实现 `IFunctionPermissionService` 的权限服务（通常基于当前登录用户的角色与功能点配置）使用。

## 依赖说明

- `YS.Knife.Function.Core`：提供 `IFunctionPermissionService`。
- `YS.Knife.Operations.Core`：提供 `Operation` 元数据解析（`GetOperation()`）。
- `YS.Knife.LogicRoles.Core`：逻辑角色相关抽象。
- `Microsoft.AspNetCore.Authorization` / MVC 过滤器基础设施。

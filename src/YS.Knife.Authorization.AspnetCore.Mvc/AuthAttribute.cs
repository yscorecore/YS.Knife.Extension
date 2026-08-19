using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using YS.Knife.Function;
using YS.Knife.Operations;
namespace YS.Knife.Authorization.AspnetCore.Mvc
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly,
                    AllowMultiple = false, Inherited = true)]
    public class AuthAttribute : Attribute, IAsyncAuthorizationFilter
    {
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {

            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<AuthAttribute>>();
            var service = context.HttpContext.RequestServices.GetRequiredService<IFunctionPermissionService>();
            // 1. Check if already authorized (handled by a previous filter)
            if (context.Result != null)
            {
                logger.LogDebug("Authorization already handled by a prior filter. Skipping current check.");
                return;
            }

            // 2. Check for AllowAnonymous
            var endpoint = context.HttpContext.GetEndpoint();
            if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null ||
                context.ActionDescriptor.EndpointMetadata?.Any(m => m is IAllowAnonymous) == true)
            {
                logger.LogDebug("Current endpoint allows anonymous access. Skipping authorization check.");
                return;
            }

            if (context.ActionDescriptor is not ControllerActionDescriptor cad)
            {
                logger.LogWarning("Not a Controller endpoint. Authorization check skipped.");
                return;
            }



            var method = cad.MethodInfo;
            var operation = method.GetOperation();


            if (await service.HasPermission(operation.AppId, operation.Id))
            {
                logger.LogInformation("User {UserName} authorized for operation {OperationId}.",
                                      context.HttpContext.User.Identity?.Name, operation.Id);
            }
            else
            {
                logger.LogWarning("User {UserName} has no permission for operation {OperationId} (App: {AppId}).",
                                  context.HttpContext.User.Identity?.Name, operation.Id, operation.AppId);
                context.Result = new ForbidResult();
            }
        }
    }
}

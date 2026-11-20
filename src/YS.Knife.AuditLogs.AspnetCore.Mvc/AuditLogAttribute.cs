using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using YS.Knife.Operations;

namespace YS.Knife.AuditLogs.AspnetCore.Mvc
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class AuditLogAttribute : ActionFilterAttribute
    {
        public AuditLogAttribute()
        {
            this.Order = -6000;
        }
        public string[] LogHttpMethods { get; set; } = new[] { "POST", "PUT", "DELETE", "PATCH" };


        public override void OnActionExecuted(ActionExecutedContext context)
        {
            base.OnActionExecuted(context);
            var auditLog = (AuditLogContext)context.HttpContext.Items[typeof(AuditLogContext)];
            if (auditLog != null)
            {
                auditLog.ReturnValue = GetResultValue(context);
                auditLog.Log.Error = context.Exception;
                auditLog.Log.Duration = (DateTimeOffset.Now - auditLog.Log.ExecutionTime).TotalMilliseconds;
                AppendExecutedData(auditLog).GetAwaiter().GetResult();
                WriteAuditLog(auditLog).GetAwaiter().GetResult();
            }

        }
        private object GetResultValue(ActionExecutedContext context)
        {
            if (context.Result is ObjectResult obj)
            {
                return obj.Value;
            }
            else
            {
                return null;
            }
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (ShouldLog(context))
            {
                var auditLog = CreateNewLogContext(context);
                context.HttpContext.Items[typeof(AuditLogContext)] = auditLog;
                AppendExecutingData(auditLog).GetAwaiter().GetResult();
            }
            base.OnActionExecuting(context);
        }
        private bool ShouldLog(ActionExecutingContext context)
        {
            if (context.HttpContext.Items.ContainsKey(typeof(AuditLog)))
            {
                return false;
            }
            if (!this.LogHttpMethods.Contains(context.HttpContext.Request.Method, StringComparer.InvariantCultureIgnoreCase))
            {
                return false;
            }
            if (context.ActionDescriptor is ControllerActionDescriptor actionDescriptor)
            {
                if (actionDescriptor.MethodInfo.GetCustomAttribute<NonAuditLogAttribute>(true) != null)
                {
                    return false;
                }
            }

            return true;
        }


        private async Task AppendExecutingData(AuditLogContext logContext)
        {
            var contextProviders = logContext.ServiceProvider.GetServices<IAutitLogExecutingDataEnricher>();
            foreach (var contextProvider in contextProviders)
            {
                await contextProvider.EnrichExecutingLogData(logContext);
            }
        }
        private async Task AppendExecutedData(AuditLogContext logContext)
        {
            var contextProviders = logContext.ServiceProvider.GetServices<IAutitLogExecutedDataEnricher>();
            foreach (var contextProvider in contextProviders)
            {
                await contextProvider.EnrichExecutedLogData(logContext);
            }
        }

        private async Task WriteAuditLog(AuditLogContext logContext)
        {
            var options = logContext.ServiceProvider.GetRequiredService<AuditLogOptions>();

            await SyncWriteAuditLog();


            async Task SyncWriteAuditLog()
            {
                var writers = logContext.ServiceProvider.GetServices<IAuditLogWriter>();
                var logger = logContext.ServiceProvider.GetRequiredService<ILogger<AuditLogAttribute>>();
                foreach (var writer in writers)
                {
                    try
                    {
                        await writer.WriteLog(logContext.Log);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "write log error.");
                    }
                }
            }

        }


        private AuditLogContext CreateNewLogContext(ActionExecutingContext context)
        {
            var sp = context.HttpContext.RequestServices;
            var logAccessor = sp.GetRequiredService<AuditLogAccessor>();
            var log = logAccessor.Current;
            log.ExecutionTime = DateTimeOffset.Now;
            if (context.ActionDescriptor is Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor action)
            {
                var operation = action.MethodInfo.GetOperation();
                log.OperationId = operation.Id;
                log.OperationDesc = operation.Description;
            }
            return new AuditLogContext
            {
                Log = logAccessor.Current,
                ServiceProvider = context.HttpContext.RequestServices,
                Arguments = context.ActionArguments,
            };
        }
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class NonAuditLogAttribute : Attribute
    {

    }
}

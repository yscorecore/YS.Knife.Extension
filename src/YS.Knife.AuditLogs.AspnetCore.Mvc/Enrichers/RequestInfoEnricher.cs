using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace YS.Knife.AuditLogs.AspnetCore.Mvc.Enrichers
{
    [Service]
    [AutoConstructor]
    internal partial class RequestInfoEnricher : IAutitLogExecutingDataEnricher
    {

        private readonly IHttpContextAccessor httpContextAccessor;
        public Task EnrichExecutingLogData(IAuditLogContext context)
        {
            var httpContext = httpContextAccessor.HttpContext;
            context.Log.PushData(AuditLogKeys.RequestIp, GetIpAddress(httpContext));
            context.Log.PushData(AuditLogKeys.User, GetUserName(httpContext));
            context.Log.PushData(AuditLogKeys.RequestUrl, httpContext.Request.Path.ToString());
            context.Log.PushData(AuditLogKeys.RequestMethod, httpContext.Request.Method);
            return Task.CompletedTask;

        }
        private string GetIpAddress(HttpContext context)
        {
            var ip = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            return string.IsNullOrEmpty(ip) ? context.Connection.RemoteIpAddress.ToString() : ip;
        }
        private string GetUserName(HttpContext context)
        {
            return context.User?.Claims?.FirstOrDefault(p => p.Type == ClaimTypes.Name)?.Value ?? string.Empty;
        }
    }
}


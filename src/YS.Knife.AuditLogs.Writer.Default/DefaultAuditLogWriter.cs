using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace YS.Knife.AuditLogs.AspnetCore.Mvc
{
    [Service]
    [AutoConstructor]
    public partial class DefaultAuditLogWriter : IAuditLogWriter
    {
        private readonly ILogger<DefaultAuditLogWriter> logger;
        public Task WriteLog(IAuditLog auditLog)
        {
            var template = $@"
-----------DefaultAuditLogWriter---------------
OperationId: {auditLog.OperationId}
OperationDesc: {auditLog.OperationDesc}
Duration:{auditLog.Duration:f0} ms
Success: {auditLog.Success}
ExecutionTime: {auditLog.ExecutionTime:yyyy-MM-dd HH:mm:ss.fff}
Error: {auditLog.Error?.Message ?? "None"}
{string.Join("\r\n", auditLog.Datas.Select(p => $"{p.Key}: {JsonSerializer.Serialize(p.Value)}"))}
-----------------------------------------------";
            logger.LogInformation(template);
            return Task.CompletedTask;
        }
    }
}

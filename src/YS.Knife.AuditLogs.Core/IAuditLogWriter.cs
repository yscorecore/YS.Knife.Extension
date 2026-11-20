namespace YS.Knife.AuditLogs
{
    public interface IAuditLogWriter
    {
        Task WriteLog(IAuditLog auditLog);
    }
}

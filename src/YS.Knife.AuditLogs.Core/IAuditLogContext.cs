namespace YS.Knife.AuditLogs
{
    public interface IAuditLogContext
    {
        object ReturnValue { get; }
        IDictionary<string, object> Arguments { get; }
        IAuditLog Log { get; }
        IServiceProvider ServiceProvider { get; }
    }
}

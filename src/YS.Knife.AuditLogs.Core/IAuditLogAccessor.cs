namespace YS.Knife.AuditLogs
{
    public interface IAuditLogAccessor
    {
        IAuditLog Log { get; }
        bool HasCreated { get; }
    }
}

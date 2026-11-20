namespace YS.Knife.AuditLogs
{
    public interface IAuditLog
    {
        IDictionary<string, object> Datas { get; }
        double Duration { get; }
        Exception Error { get; }
        DateTimeOffset ExecutionTime { get; }
        string OperationId { get; }
        string OperationDesc { get; }
        bool Success { get; }
    }
}

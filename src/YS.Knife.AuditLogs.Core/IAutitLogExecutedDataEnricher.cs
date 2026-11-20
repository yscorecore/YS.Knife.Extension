namespace YS.Knife.AuditLogs
{
    public interface IAutitLogExecutedDataEnricher
    {
        Task EnrichExecutedLogData(IAuditLogContext context);
    }
}

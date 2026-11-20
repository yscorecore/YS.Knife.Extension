namespace YS.Knife.AuditLogs
{
    public interface IAutitLogExecutingDataEnricher
    {
        Task EnrichExecutingLogData(IAuditLogContext context);
    }
}

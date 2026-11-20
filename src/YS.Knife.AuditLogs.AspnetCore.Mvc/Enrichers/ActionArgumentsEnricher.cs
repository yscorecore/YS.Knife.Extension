namespace YS.Knife.AuditLogs.AspnetCore.Mvc.Enrichers
{
    [Service]
    [AutoConstructor]
    internal partial class ActionArgumentsEnricher : IAutitLogExecutingDataEnricher
    {
        private readonly AuditLogOptions options;
        public Task EnrichExecutingLogData(IAuditLogContext context)
        {
            var input = context.Arguments.Where(p =>
            {
                var valueType = p.Value?.GetType();
                if (valueType != null && options.IgnoreDataTypes.Any(t => t.IsAssignableFrom(valueType)))
                {
                    return false;
                }
                return true;
            }).ToDictionary(p => p.Key, p => p.Value);
            context.Log.PushData(AuditLogKeys.RequestObject, input);
            return Task.CompletedTask;
        }
    }
}

namespace YS.Knife.AuditLogs.AspnetCore.Mvc
{
    internal class AuditLogContext : IAuditLogContext
    {
        public IDictionary<string, object> Arguments { get; set; }

        public AuditLog Log { get; set; }

        public IServiceProvider ServiceProvider { get; set; }

        public object ReturnValue { get; set; }
        IAuditLog IAuditLogContext.Log => this.Log;
    }
}

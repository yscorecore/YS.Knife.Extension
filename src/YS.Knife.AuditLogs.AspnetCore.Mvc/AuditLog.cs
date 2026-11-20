namespace YS.Knife.AuditLogs
{
    internal record AuditLog : IAuditLog
    {
        public DateTimeOffset ExecutionTime { get; set; }

        public string OperationId { get; set; }
        public string OperationDesc { get; set; }

        public double Duration { get; set; } //间隔时长,毫秒

        public bool Success { get => Error == null; }

        public Exception Error { get; set; }

        public IDictionary<string, object> Datas { get; } = new Dictionary<string, object>();


    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YS.Knife.AuditLogs.AspnetCore.Mvc.Enrichers
{
    [Service]
    [AutoConstructor]
    internal partial class ActionResultEnricher : IAutitLogExecutedDataEnricher
    {
        public Task EnrichExecutedLogData(IAuditLogContext context)
        {
            context.Log.PushData(AuditLogKeys.ResponseObject, context.ReturnValue);
            return Task.CompletedTask;
        }

    }
}

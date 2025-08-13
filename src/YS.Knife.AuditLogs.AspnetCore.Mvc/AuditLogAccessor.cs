using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YS.Knife.AuditLogs.AspnetCore.Mvc
{
    internal class AuditLogAccessor : IAuditLogAccessor
    {
        private Lazy<AuditLog> factory = new Lazy<AuditLog>(() => new AuditLog
        {

        }, true);
        public AuditLog Current => factory.Value;

        public IAuditLog Log => factory.Value;

        public bool HasCreated => factory.IsValueCreated;
    }
}

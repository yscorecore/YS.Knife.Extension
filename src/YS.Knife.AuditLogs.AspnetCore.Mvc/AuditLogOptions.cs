using System.ComponentModel.DataAnnotations;

namespace YS.Knife.AuditLogs.AspnetCore.Mvc
{
    [Options]
    public class AuditLogOptions
    {
        [Required]
        public List<Type> IgnoreDataTypes { get; set; } = new List<Type> { typeof(CancellationToken), typeof(Stream) };

    }
}

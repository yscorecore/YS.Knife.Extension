using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace YS.Knife.AuditLogs.AspnetCore.Mvc
{
    internal class ServiceRegister : YS.Knife.IServiceRegister
    {
        public void RegisterServices(IServiceCollection services, IRegisterContext context)
        {
            services.AddScoped<AuditLogAccessor, AuditLogAccessor>();
            services.AddScoped<IAuditLogAccessor>(sp => sp.GetService<AuditLogAccessor>());
            services.Configure<AuditLogOptions>(p =>
            {
                p.IgnoreDataTypes = new List<Type> { typeof(CancellationToken), typeof(Stream), typeof(IFormFile), typeof(IFormFileCollection), typeof(IEnumerable<IFormFile>) };
            });
        }
    }
}

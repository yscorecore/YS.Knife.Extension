
using Microsoft.AspNetCore.Mvc;
using YS.Knife.AuditLogs.AspnetCore.Mvc;

namespace AuditLogDemo
{
    public class Program : YS.Knife.Hosting.KnifeWebHost
    {
        public Program(string[] args) : base(args)
        {
        }
        public static void Main(string[] args)
        {
            new Program(args).Run();
        }
        protected override void OnConfigureCustomService(HostBuilderContext builder, IServiceCollection serviceCollection)
        {
            serviceCollection.Configure<MvcOptions>(options =>
            {
                options.Filters.Add<AuditLogAttribute>();
            });
            base.OnConfigureCustomService(builder, serviceCollection);
        }
    }
}

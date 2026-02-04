using Microsoft.Extensions.DependencyInjection;

namespace YS.Knife.KeyValue.Default
{
    class ServiceRegister : IServiceRegister
    {
        public void RegisterServices(IServiceCollection services, IRegisterContext context)
        {
            services.AddScoped(typeof(IKeyValueService<>), typeof(KeyValueGroupService<>));
        }
    }
}

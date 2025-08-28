using Microsoft.Extensions.DependencyInjection;
namespace YS.Knife.Lock.Impl.Memory
{
    class ServiceRegister : IServiceRegister
    {
        public void RegisterServices(IServiceCollection services, IRegisterContext context)
        {
            services.AddMemoryCache();
        }
    }
}

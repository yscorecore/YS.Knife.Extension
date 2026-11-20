using Microsoft.Extensions.DependencyInjection;
using YS.Knife.Resource.AppFileResLoader;

namespace YS.Knife.Resource
{
    internal class ServiceRegister : IServiceRegister
    {
        public void RegisterServices(IServiceCollection services, IRegisterContext context)
        {
            services.AddHttpClient<AppResourceLoader>();
            services.AddHttpClient<AppResourceNameGroupLoader>();
        }
    }
}

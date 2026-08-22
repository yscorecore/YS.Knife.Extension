using Microsoft.Extensions.DependencyInjection;

namespace YS.Knife.Resource
{
    internal class ServiceRegister : IServiceRegister
    {
        public void RegisterServices(IServiceCollection services, IRegisterContext context)
        {
            services.AddHttpClient<HttpResourceLoader>();
        }
    }
}

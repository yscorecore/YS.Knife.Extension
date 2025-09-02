using Microsoft.Extensions.DependencyInjection;
using YS.Knife;

namespace YS.Knife.Sms.Impl.Submail.IntegrationTest
{
    internal class ServiceRegister : IServiceRegister
    {
        public void RegisterServices(IServiceCollection services, IRegisterContext context)
        {
            services.AddHttpClient();
        }
    }
}

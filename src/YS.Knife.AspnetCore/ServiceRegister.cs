
using System.Text.Json;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace YS.Knife.AspnetCore
{
    public class ServiceRegister : IServiceRegister
    {
        public void RegisterServices(IServiceCollection services, IRegisterContext context)
        {
            services.AddScoped<JsonSerializerOptions>(p =>
            {
                var options = p.GetRequiredService<IOptions<JsonOptions>>();
                return options.Value.SerializerOptions;
            });

            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }
    }
}

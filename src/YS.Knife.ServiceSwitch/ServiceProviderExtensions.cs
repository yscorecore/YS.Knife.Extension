using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace System
{
    public static class ServiceProviderExtensions
    {
        public static T GetServiceByName<T>(this IServiceProvider serviceProvider, string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return serviceProvider.GetRequiredService<T>();
            }
            else
            {
                var services = serviceProvider.GetRequiredService<IDictionary<string, T>>();
                if (services.TryGetValue(name, out var s))
                {
                    return s;
                }
                throw new Exception($"Can not find service '{typeof(T).FullName}' by name '{name}'");
            }
        }
        public static T GetServiceByNameOrConfiguationSwitch<T>(this IServiceProvider serviceProvider, string name = default)
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var serviceName = name ??
                configuration[$"ServiceSwitch:{typeof(T).Name}"]
                ?? configuration[$"ServiceSwitch:{typeof(T).FullName}"];
            return GetServiceByName<T>(serviceProvider, serviceName);
        }
        public static T GetServiceByConfiguationSwitch<T>(this IServiceProvider serviceProvider)
        {
            return GetServiceByNameOrConfiguationSwitch<T>(serviceProvider, null);
        }
    }
}

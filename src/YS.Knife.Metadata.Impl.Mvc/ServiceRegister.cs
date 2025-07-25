using System.Reflection;
using YS.Knife.Metadata.Impl.Mvc;

namespace YS.Knife.Metadata.Provider.MvcMetadata
{
    internal class ServiceRegister : IServiceRegister
    {
        public void RegisterServices(IServiceCollection services, IRegisterContext context)
        {
            var metaClasses = AppDomain.CurrentDomain.GetAssemblies()
                .Where(p => !p.IsFromMicrosoft())
                .SelectMany(p => p.GetTypes().Where(t => t.IsClass && !t.IsAbstract && Attribute.IsDefined(t, typeof(MetadataAttribute))))
                .Select(p => new
                {
                    Name = p.GetCustomAttribute<MetadataAttribute>()?.Name ?? p.FullName ?? "",
                    Type = p
                });
            services.AddOptions<MetadataOptions>()
                .Configure(options =>
                {
                    foreach (var item in metaClasses)
                    {
                        options.Metas[item.Name] = item.Type;
                    }
                });
        }
    }
}

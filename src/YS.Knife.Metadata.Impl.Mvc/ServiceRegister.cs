
using System.Reflection;

namespace YS.Knife.Metadata.Impl.Mvc
{
    public class ServiceRegister : YS.Knife.IServiceRegister
    {
        public void RegisterServices(IServiceCollection services, IRegisterContext context)
        {
            var allTypes = AppDomain.CurrentDomain.GetAssemblies()
                  .Where(p => !p.IsFromMicrosoft())
                  .SelectMany(p => p.GetTypes())
                  .Where(p => p.IsClass && !p.IsAbstract && Attribute.IsDefined(p, typeof(MetadataAttribute)))
                  .Where(p => !context.HasFiltered(p))
                  .Select(p => (p, p.GetCustomAttribute<MetadataAttribute>()))
                  .ToList();
            services.Configure<MetadataOptions>(t =>
            {
                foreach (var type in allTypes)
                {
                    var name = type.Item2.Name ?? type.p.FullName;
                    t.AddMeta(name, type.p);
                }
            });
        }
    }
}

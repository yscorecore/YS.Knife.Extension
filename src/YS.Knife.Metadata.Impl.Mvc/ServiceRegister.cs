
using System.Reflection;

namespace YS.Knife.Metadata.Impl.Mvc
{
    public class ServiceRegister : YS.Knife.IServiceRegister
    {
        public void RegisterServices(IServiceCollection services, IRegisterContext context)
        {
            var allTypes = AppDomain.CurrentDomain.FindInstanceTypesByAttribute<MetadataAttribute>()
                  .ToArray();
            services.Configure<MetadataOptions>(t =>
            {
                t.AddOrReplaceMeta(allTypes);
            });
        }
    }
}

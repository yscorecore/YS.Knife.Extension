using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using YS.Knife.Metadata;
using YS.Knife.Query;

namespace YS.Knife.DataSource.Impl.Default
{
    internal class ServiceRegister : IServiceRegister
    {
        public void RegisterServices(IServiceCollection services, IRegisterContext context)
        {
            RegisterDataMetas(services, context);
        }

        private void RegisterDataMetas(IServiceCollection services, IRegisterContext context)
        {
            services.AddOptions<MetadataOptions>().Configure((o) =>
            {
                foreach (var item in AssemblyDataSourceEntryFinder.Instance.All.Values.Where(p => p.AutoRegisterMeta))
                {
                    o.AddMeta(item.Name, item.EntityType);
                }
            });
        }

    }
}

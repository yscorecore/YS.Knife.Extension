using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using YS.Knife.AspnetCore.Mvc;

namespace AspnetCoreDemo
{
    public class Program : YS.Knife.Hosting.KnifeWebHost
    {
        public Program(string[] args) : base(args)
        {
        }
        public static void Main(string[] args)
        {
            new Program(args).Run();
        }
        protected override void OnConfigureCustomService(HostBuilderContext builder, IServiceCollection serviceCollection)
        {
            serviceCollection.Configure<MvcOptions>(options =>
            {
                options.Filters.Add<WrapCodeResultAttribute>();
            });
            serviceCollection.AddMvc().ConfigureApplicationPartManager(manager =>
            {
                manager.FeatureProviders.Add(new GenericControllerFeatureProvider());
            });
            base.OnConfigureCustomService(builder, serviceCollection);

        }

    }
}

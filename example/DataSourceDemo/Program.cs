using YS.Knife.AspnetCore;
using YS.Knife.AspnetCore.Mvc;

namespace DataSourceDemo
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
            serviceCollection.AddMvc().ConfigureApplicationPartManager(manager =>
            {
                manager.FeatureProviders.Add(new GenericControllerFeatureProvider());
            });
            base.OnConfigureCustomService(builder, serviceCollection);
        }



    }
}

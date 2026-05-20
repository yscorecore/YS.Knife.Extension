using YS.Knife.AspnetCore.Mvc;

namespace DataItemDemo
{
    [ExposeDataItemApi(typeof(IService1), nameof(IService1.GetTaskValue))]
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
    [System.AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    sealed class ExposeDataItemApiAttribute : Attribute
    {

        public ExposeDataItemApiAttribute(Type serviceType, params string[] methods)
        {
            this.Methods = methods;
        }
        public Type ServiceType { get; set; }
        public string[] Methods { get; set; }
    }
}

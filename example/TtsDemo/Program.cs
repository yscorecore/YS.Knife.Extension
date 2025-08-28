

namespace TtsDemo
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
            base.OnConfigureCustomService(builder, serviceCollection);
            serviceCollection.AddDistributedMemoryCache();
            serviceCollection.AddHttpClient();
        }
    }
}

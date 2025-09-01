


using Microsoft.AspNetCore.HttpOverrides;

namespace FileStorageDemo
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

        }
        protected override void ConfigureWebApp(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UsePathBase("/myapp");
            app.UseStaticFiles();
            base.ConfigureWebApp(app, env);

        }
    }
}

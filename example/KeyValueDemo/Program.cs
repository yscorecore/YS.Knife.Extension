
using Microsoft.EntityFrameworkCore;
using YS.Knife.AspnetCore.Mvc;

namespace KeyValueDemo
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
            serviceCollection.AddDbContext<KeyValueContext>((op) =>
            {
                op.UseSqlite("Data Source=demo.db").EnableSensitiveDataLogging(true);
                op.LogTo(Console.WriteLine);
            });
        }
        protected override void ConfigureWebApp(IApplicationBuilder app, IWebHostEnvironment env)
        {
            base.ConfigureWebApp(app, env);
            var scopeFactory = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>();
            using var scope = scopeFactory.CreateScope();
            using (var context = scope.ServiceProvider.GetService<KeyValueContext>())
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

            }
        }
    }
}

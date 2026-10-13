
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TagsDemo
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
            serviceCollection.AddDbContext<TagDbContext>((op) =>
            {
                //op.UseSqlite("Data Source=:memory:").EnableSensitiveDataLogging(true);
                op.UseSqlite("Data Source=abc.db.tmp").EnableSensitiveDataLogging(true);
                op.LogTo(Console.WriteLine);
            });
        }
        protected override void ConfigureWebApp(IApplicationBuilder app, IWebHostEnvironment env)
        {
            base.ConfigureWebApp(app, env);
            var scopeFactory = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>();
            using var scope = scopeFactory.CreateScope();
            using (var context = scope.ServiceProvider.GetService<TagDbContext>())
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

            }
        }

    }
}

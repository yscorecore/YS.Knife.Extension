using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MixinDemo.Core;
using MixinDemo.Model;
using YS.Knife;
using YS.Knife.AspnetCore.Mvc;
using YS.Knife.AuditLogs.AspnetCore.Mvc;

namespace MixinDemo
{
    [ExposeApi(typeof(ILabelService), TypeAttributePatterns = new string[] { "*" }, MethodAttributePatterns = new string[] { "*" })]
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

            serviceCollection.AddFlyTigerMapper(typeof(Program));
            serviceCollection.Configure<MvcOptions>(options =>
            {
                options.Filters.Add<AuditLogAttribute>();
                options.Filters.Add<WrapCodeResultAttribute>();
            });
            serviceCollection.AddMvc().ConfigureApplicationPartManager(manager =>
            {
                manager.FeatureProviders.Add(new GenericControllerFeatureProvider());
            });
            base.OnConfigureCustomService(builder, serviceCollection);
            serviceCollection.AddDbContext<DemoContext>((op) =>
            {
                op.UseSqlite("Data Source=demo.db.tmp").EnableSensitiveDataLogging(true);
                // op.UseSqlite("Data Source=:memory:").EnableSensitiveDataLogging(true);
                op.LogTo(Console.WriteLine);
            });

        }
        protected override void ConfigureWebApp(IApplicationBuilder app, IWebHostEnvironment env)
        {
            base.ConfigureWebApp(app, env);
            var scopeFactory = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>();
            using var scope = scopeFactory.CreateScope();
            using (var context = scope.ServiceProvider.GetRequiredService<DemoContext>())
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
                Enumerable.Range(1, 100).ToList().ForEach(i =>
                {
                    context.Labels.Add(new LabelEntity()
                    {
                        Name = $"Label {i}",
                        Desc = $"Description {i}",
                        CreateTime = DateTime.Now,
                        Value = Random.Shared.NextDouble() * 100,
                        Status = (LabelStatus)Random.Shared.Next(0, 3)
                    });
                });
                context.SaveChanges();
            }
        }
    }
}

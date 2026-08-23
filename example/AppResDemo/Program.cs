
using AppResDemo.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YS.Knife.AppRes;
using YS.Knife.AppRes.Impl.EFCore;

namespace AppResDemo
{
    [YS.Knife.ExposeApi(typeof(IAppTextResourceService))]
    [YS.Knife.ExposeApi(typeof(IAppTextResourceManager))]
    [YS.Knife.ExposeApi(typeof(IAppFileResourceService))]
    [YS.Knife.ExposeApi(typeof(IAppFileResourceManager))]
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

            //serviceCollection.AddFlyTigerMapper(typeof(Program));
            //serviceCollection.Configure<MvcOptions>(options =>
            //{
            //    options.Filters.Add<AuditLogAttribute>();
            //    options.Filters.Add<WrapCodeResultAttribute>();
            //});
            serviceCollection.AddFlyTigerMapper(typeof(AppFileResourceManager));
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
                    //context.Labels.Add(new LabelEntity()
                    //{
                    //    Name = $"Label {i}",
                    //    Desc = $"Description {i}",
                    //    CreateTime = DateTime.Now,
                    //    Value = Random.Shared.NextDouble() * 100,
                    //    Status = (LabelStatus)Random.Shared.Next(0, 3)
                    //});
                });
                context.SaveChanges();
            }
        }
    }
}

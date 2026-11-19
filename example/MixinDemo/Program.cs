using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MixinDemo.Core;
using MixinDemo.Model;
using YS.Knife;
using YS.Knife.AspnetCore.Mvc;
using YS.Knife.AuditLogs.AspnetCore.Mvc;

using YS.Knife.Hosting;

namespace MixinDemo
{
    [ExposeApi(typeof(ILabelService), TypeAttributePatterns = new string[] { "*" }, MethodAttributePatterns = new string[] { "*" })]
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddAllKnifeServices(builder.Configuration);

            builder.Services.AddFlyTigerMapper(typeof(Program));

            builder.Services.AddSwaggerGen();
            builder.Services.Configure<MvcOptions>(options =>
            {
                options.Filters.Add<AuditLogAttribute>();
                options.Filters.Add<WrapCodeResultAttribute>();
            });
            builder.Services.AddControllers().ConfigureApplicationPartManager(manager =>
            {
                manager.FeatureProviders.Add(new GenericControllerFeatureProvider());
            });
            builder.Services.AddDbContext<DemoContext>(op =>
            {
                op.UseSqlite("Data Source=demo.db.tmp").EnableSensitiveDataLogging(true);
                op.LogTo(Console.WriteLine);
            });

            var app = builder.Build();
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();

                app.UseDeveloperExceptionPage();
            }
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            app.MapControllers();

            // 初始化数据库 + 种子数据
            using (var scope = app.Services.CreateScope())
            {
                using var context = scope.ServiceProvider.GetRequiredService<DemoContext>();
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

            app.Run();
        }
    }
}

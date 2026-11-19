using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YS.Knife.AspnetCore.Mvc;

using YS.Knife.Hosting;

namespace KeyValueDemo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddAllKnifeServices(builder.Configuration);


            builder.Services.AddSwaggerGen();
            builder.Services.Configure<MvcOptions>(options =>
            {
                options.Filters.Add<WrapCodeResultAttribute>();
            });
            builder.Services.AddControllers().ConfigureApplicationPartManager(manager =>
            {
                manager.FeatureProviders.Add(new GenericControllerFeatureProvider());
            });
            builder.Services.AddDbContext<KeyValueContext>(op =>
            {
                op.UseSqlite("Data Source=abc.db.tmp").EnableSensitiveDataLogging(true);
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

            // 初始化数据库
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<KeyValueContext>();
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
            }

            app.Run();
        }
    }
}

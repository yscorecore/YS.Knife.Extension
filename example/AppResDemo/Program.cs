using AppResDemo.Model;
using Microsoft.EntityFrameworkCore;
using YS.Knife.AppRes;
using YS.Knife.AppRes.Impl.EFCore;

using YS.Knife.Hosting;

namespace AppResDemo
{
    [YS.Knife.ExposeApi(typeof(IAppTextResourceService))]
    [YS.Knife.ExposeApi(typeof(IAppTextResourceManager))]
    [YS.Knife.ExposeApi(typeof(IAppFileResourceService))]
    [YS.Knife.ExposeApi(typeof(IAppFileResourceManager))]
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddAllKnifeServices(builder.Configuration);

            builder.Services.AddFlyTigerMapper(typeof(AppFileResourceManager));

            builder.Services.AddSwaggerGen();
            builder.Services.AddDbContext<DemoContext>(op =>
            {
                op.UseSqlite("Data Source=demo.db.tmp").EnableSensitiveDataLogging(true);
                op.LogTo(Console.WriteLine);
            });
            builder.Services.AddControllers();

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
            app.Run();
        }
    }
}

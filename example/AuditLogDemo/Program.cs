using Microsoft.AspNetCore.Mvc;
using YS.Knife.AuditLogs.AspnetCore.Mvc;

using YS.Knife.Hosting;

namespace AuditLogDemo
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
                options.Filters.Add<AuditLogAttribute>();
            });
            builder.Services.AddControllers();

            var app = builder.Build();
            app.MapControllers();
            app.UseSwagger();
            app.UseSwaggerUI();

            app.Run();
        }
    }
}

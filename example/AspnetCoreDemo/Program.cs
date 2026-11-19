using Microsoft.AspNetCore.Mvc;
using YS.Knife.AspnetCore.Mvc;
using YS.Knife.Hosting;

namespace AspnetCoreDemo
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

            var app = builder.Build();
            app.MapControllers();
            app.UseSwagger();
            app.UseSwaggerUI();

            app.Run();
        }
    }
}

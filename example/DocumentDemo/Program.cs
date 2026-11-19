using YS.Knife.Document;

using YS.Knife.Hosting;

namespace DocumentDemo
{
    [YS.Knife.ExposeApi(typeof(IDocTypeConvertService))]
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddAllKnifeServices(builder.Configuration);
            builder.Services.AddControllers();


            builder.Services.AddSwaggerGen();
            var app = builder.Build();
            app.MapControllers();
            app.UseSwagger();
            app.UseSwaggerUI();

            app.Run();
        }
    }
}

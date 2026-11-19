using YS.Knife.Hosting;

namespace TtsDemo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddAllKnifeServices(builder.Configuration);

            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddHttpClient();
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

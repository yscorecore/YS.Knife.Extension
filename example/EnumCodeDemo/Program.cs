using YS.Knife.EnumCode;

using YS.Knife.Hosting;

namespace EnumCodeDemo
{
    [YS.Knife.ExposeApi(typeof(IEnumCodeService))]
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

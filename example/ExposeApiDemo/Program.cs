using YS.Knife;
using YS.Knife.Time;
namespace ExposeApiDemo
{
    [ExposeApi(typeof(IService1))]
    [ExposeApi(typeof(ITimeService), AllowAnonymous = true)]
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddMvc();
            // Add Swagger services with explicit configuration
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "ExposeApiDemo", Version = "v1" });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            // Add Swagger middleware
            app.UseSwagger();
            app.UseSwaggerUI();


            var summaries = new[]
            {
                "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
            };
            app.MapControllers();
            app.MapGet("/weatherforecast", (HttpContext httpContext) =>
            {
                var forecast = Enumerable.Range(1, 5).Select(index =>
                    new WeatherForecast
                    {
                        Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                        TemperatureC = Random.Shared.Next(-20, 55),
                        Summary = summaries[Random.Shared.Next(summaries.Length)]
                    })
                    .ToArray();
                return forecast;
            });

            app.Run();
        }
    }

    public interface IService1
    {
        string GetData(int id, CancellationToken token);

        Task<string> GetData2(int id);

        ValueTask<string> GetData3(int id);

        Task<string> ModifyAbc(string abc);

        Task CreateNew();
    }
    public interface IService2
    {
        void SaveData(string data);
    }
}

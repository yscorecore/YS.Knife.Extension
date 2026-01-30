using System;
using YS.Knife;

namespace ExposeApiDemo
{
    [ExposeApi(typeof(IService1))]
    [ExposeApi(typeof(MyService))]
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

                // 启用XML注释
                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
                // 启用Swagger注解支持
                c.EnableAnnotations();
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
            // 使用Swagger特性丰富文档信息
            app.MapGet("/weatherforecast", [Swashbuckle.AspNetCore.Annotations.SwaggerOperation(
                Summary = "获取天气预报",
                Description = "获取未来5天的天气预报信息，包括日期、温度和天气概况"
            ), Swashbuckle.AspNetCore.Annotations.SwaggerResponse(200, "成功获取天气预报", typeof(IEnumerable<WeatherForecast>))]
            (HttpContext httpContext) =>
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

    /// <summary>
    /// 服务接口1，提供数据获取和修改功能
    /// </summary>
    public interface IService1
    {
        /// <summary>
        /// 根据ID获取数据
        /// </summary>
        /// <param name="id">数据ID</param>
        /// <param name="token">取消令牌</param>
        /// <returns>数据内容</returns>
        string GetData(int id, CancellationToken token);

        /// <summary>
        /// 根据ID获取数据（异步版本2）
        /// </summary>
        /// <param name="id">数据ID</param>
        /// <returns>数据内容</returns>
        Task<string> GetData2(int id);

        /// <summary>
        /// 根据ID获取数据（异步版本3）
        /// </summary>
        /// <param name="id">数据ID</param>
        /// <returns>数据内容</returns>
        ValueTask<string> GetData3(int id);

        /// <summary>
        /// 修改Abc数据
        /// </summary>
        /// <param name="abc">新的Abc内容</param>
        /// <returns>修改后的数据</returns>
        Task<string> ModifyAbc(string abc);
        /// <summary>
        /// 创建新数据
        /// </summary>
        Task CreateNew();
    }
    public interface IService2
    {
        void SaveData(string data);
    }

    public class MyService
    {
        private Task SayHello()
        {
            return Task.CompletedTask;
        }
        public Task SayHelloPublic()
        {
            return SayHello();
        }
    }
}

using DataSourceDemo.Controllers;
using YS.Knife.AspnetCore.Mvc;
using YS.Knife.DataSource.AspnetCore;
using YS.Knife.Hosting;
using YS.Knife.Query;

namespace DataSourceDemo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddAllKnifeServices(builder.Configuration);


            builder.Services.AddSwaggerGen();
            builder.Services.AddControllers().ConfigureApplicationPartManager(manager =>
            {
                manager.FeatureProviders.Add(new GenericControllerFeatureProvider());
            });
            builder.Services.Configure<DataSourceOptions>(options =>
            {
                //通过代码方式添加数据源配置
                options.AddDataSource<OrdersController, Order>("orders", p => p.GetDataAsync(It.Any<LimitQueryInfo>(), It.Any<CancellationToken>()))
                    .AddDataSource<ProductsController, Product>("products", p => p.GetDataAsync(It.Any<LimitQueryInfo>()))
                    .AddDataSource<ProductsController, Product>("books", p => p.GetDataAsync(It.Any<LimitQueryInfo>()), p => p.Category == "books")
                    .AddDataSource<ProductsController, Product>("electronics", p => p.GetDataAsync(It.Any<LimitQueryInfo>()), p => p.Category == "electronics");
            });

            var app = builder.Build();
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();

                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseDataSources();
            app.UseRouting();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}

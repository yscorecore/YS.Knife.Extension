using DataSourceDemo.Controllers;
using Microsoft.AspNetCore.Builder;
using YS.Knife.AspnetCore;
using YS.Knife.AspnetCore.Mvc;
using YS.Knife.DataSource.AspnetCore;
using YS.Knife.Hosting.Web;
using YS.Knife.Hosting.Web.Health;
using YS.Knife.Hosting.Web.Swagger;
using YS.Knife.Query;

namespace DataSourceDemo
{
    public class Program : YS.Knife.Hosting.KnifeWebHost
    {
        public Program(string[] args) : base(args)
        {
        }
        public static void Main(string[] args)
        {
            new Program(args).Run();
        }

        protected override void OnConfigureCustomService(HostBuilderContext builder, IServiceCollection serviceCollection)
        {
            serviceCollection.AddMvc().ConfigureApplicationPartManager(manager =>
            {
                manager.FeatureProviders.Add(new GenericControllerFeatureProvider());
            });
            base.OnConfigureCustomService(builder, serviceCollection);
            serviceCollection.Configure<DataSourceOptions>(options =>
            {
                //通过代码方式添加数据源配置
                options.AddDataSource<OrdersController, Order>("orders", p => p.GetDataAsync(It.Any<LimitQueryInfo>(), It.Any<CancellationToken>()))
                    .AddDataSource<ProductsController, Product>("products", p => p.GetDataAsync(It.Any<LimitQueryInfo>()))
                    .AddDataSource<ProductsController, Product>("books", p => p.GetDataAsync(It.Any<LimitQueryInfo>()), p => p.Category == "books")
                    .AddDataSource<ProductsController, Product>("electronics", p => p.GetDataAsync(It.Any<LimitQueryInfo>()), p => p.Category == "electronics");

            });

        }
        protected override void ConfigureWebApp(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseResponseTraceId();

            app.UseRequestLogging();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }


            app.UseKnifeSwagger();

            app.UseHttpsRedirection();
            app.UseDataSources();
            //app.UseCorrelationId();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDataSources();
                endpoints.MapControllers();
                endpoints.MapKnifeHealthCheckWhenProvided();
            });
        }

        //protected override void ConfigureWebApp(IApplicationBuilder app, IWebHostEnvironment env)
        //{
        //    // Configure the HTTP request pipeline.
        //    if (env.IsDevelopment())
        //    {
        //        app.UseSwagger();
        //        app.UseSwaggerUI();
        //    }

        //    app.UseHttpsRedirection();

        //    // 从 DataSourceOptions 动态构建 URL 重写规则并注册到 pipeline
        //    // (内部已封装 UseRewriter,必须在 UseRouting 之前)
        //    app.UseDataSources();

        //    app.UseRouting();

        //    app.UseAuthorization();

        //    app.UseEndpoints(endpoints =>
        //    {
        //        endpoints.MapControllers();
        //        endpoints.MapKnifeHealthCheckWhenProvided();
        //    });
        //}


    }
}

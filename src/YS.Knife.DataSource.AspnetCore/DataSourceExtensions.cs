using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Options;
using YS.Knife.DataSource.AspnetCore;
using YS.Knife.Query;

namespace Microsoft.AspNetCore.Builder
{

    public static class DataSourceExtensions
    {

        public static IApplicationBuilder UseDataSources(this IApplicationBuilder app)
        {
            var rw = new RewriteOptions();
            return app.UseDataSources(rw).UseRewriter(rw);
        }

        /// <summary>
        /// 把 <see cref="DataSourceOptions"/> 里的重写规则追加到 <paramref name="rewriteOptions"/>,
        /// 供调用方把自己的重写规则和 datasource 规则合并进同一个 RewriteOptions(单中间件实例)。
        /// 同时注册元数据端点(见 <see cref="MapDataSources"/>)。
        ///
        /// 注意:元数据端点(/api/datasource/all 和占位端点)只有在
        /// <c>app is WebApplication</c>(最小托管)时才会自动注册。
        /// 经典 Startup 托管(Host.CreateDefaultBuilder + UseStartup)下 Configure 收到的是普通
        /// IApplicationBuilder,自动注册做不了,需要在 UseEndpoints 里手动调用
        /// <see cref="MapDataSources"/>,否则 /api/datasource/all 会 404。
        /// </summary>
        public static IApplicationBuilder UseDataSources(this IApplicationBuilder app, RewriteOptions rewriteOptions)
        {
            var options = app.ApplicationServices.GetRequiredService<IOptions<DataSourceOptions>>().Value;
            rewriteOptions.AddDataSourceRewrites(options);
            MapDataSourceEndpoints(app, options);
            return app;
        }


        public static RewriteOptions AddDataSourceRewrites(
            this RewriteOptions rewriteOptions,
            DataSourceOptions options)
        {
            ArgumentNullException.ThrowIfNull(rewriteOptions);
            ArgumentNullException.ThrowIfNull(options);

            var template = options.DataSourceEndpointTemplate;
            if (string.IsNullOrWhiteSpace(template) ||
                options.DataSources is not { Count: > 0 } sources)
            {
                return rewriteOptions;
            }

            foreach (var (name, info) in sources)
            {
                if (string.IsNullOrWhiteSpace(name) ||
                    info is null ||
                    string.IsNullOrWhiteSpace(info.EndPointPath))
                {
                    continue;
                }

                var sourcePath = string.Format(template, name);
                var finalTarget = info.EndPointPath.Trim('/');

                if (string.IsNullOrWhiteSpace(info.Filter))
                {
                    // 规则 1:没有配置 Filter -> 纯路径重写,query string 一律不动
                    var pattern = "^" + Regex.Escape(sourcePath) + "/?$";
                    rewriteOptions.AddRewrite(pattern, finalTarget, skipRemainingRules: false);
                }
                else
                {
                    // 规则 2/3:要读请求的 query string 决定"注入"还是"合并",静态正则做不到
                    rewriteOptions.Add(new DataSourceRewriteRule(sourcePath, finalTarget, info.Filter!));
                }
            }

            return rewriteOptions;
        }

        /// <summary>
        /// <see cref="AddDataSourceRewrites(Microsoft.AspNetCore.Rewrite.RewriteOptions, DataSourceOptions)"/>
        /// 的 <see cref="IOptions{TOptions}"/> 重载,便于从 DI 容器直接拿到 options。
        /// </summary>
        public static RewriteOptions AddDataSourceRewrites(
            this RewriteOptions rewriteOptions,
            IOptions<DataSourceOptions> options)
        {
            ArgumentNullException.ThrowIfNull(options);
            return rewriteOptions.AddDataSourceRewrites(options.Value);
        }

        /// <summary>
        /// 注册 datasource 元数据端点:
        ///   1) GET {AllDataSourceEndPoint} -> 所有 datasource 定义(前端发现用)
        ///   2) GET {DataSourceEndpointTemplate} -> 占位端点(字典外的 name 会走到这里并报错)
        ///
        /// 最小托管下 <see cref="UseDataSources"/> 已自动调用,无需再调;
        /// 经典 Startup 托管(Host.CreateDefaultBuilder + UseStartup)下 Configure 收到的
        /// IApplicationBuilder 不是 WebApplication,自动注册不了,需要自己在 UseEndpoints 里调用:
        /// <code>
        /// app.UseEndpoints(endpoints =>
        /// {
        ///     endpoints.MapDataSources();      // <- 补上这一行
        ///     endpoints.MapControllers();
        /// });
        /// </code>
        /// </summary>
        public static IEndpointRouteBuilder MapDataSources(this IEndpointRouteBuilder endpoints)
        {
            ArgumentNullException.ThrowIfNull(endpoints);
            var options = endpoints.ServiceProvider.GetRequiredService<IOptions<DataSourceOptions>>().Value;
            MapDataSourceEndpoints(endpoints, options);
            return endpoints;
        }

        private static void MapDataSourceEndpoints(IApplicationBuilder app, DataSourceOptions options)
        {
            // 最小托管:WebApplication 本身就是 IEndpointRouteBuilder,直接映射;
            // 经典 Startup 托管下走不进来(app 是普通 ApplicationBuilder),
            // 由调用方在 UseEndpoints 里显式调用 MapDataSources()
            if (app is not WebApplication webApp)
            {
                return;
            }

            // 显式按 IEndpointRouteBuilder 调用:WebApplication 同时实现 IApplicationBuilder,
            // 不转型的话两个私有重载会产生二义性调用 (CS0121)
            MapDataSourceEndpoints((IEndpointRouteBuilder)webApp, options);
        }

        private static void MapDataSourceEndpoints(IEndpointRouteBuilder endpoints, DataSourceOptions options)
        {
            // 1) 总览端点:GET {AllDataSourceEndPoint} -> DataSources 字典
            if (options.MapAllDataSourceEndPoint &&
                !string.IsNullOrWhiteSpace(options.AllDataSourceEndPoint))
            {
                endpoints.MapGet(options.AllDataSourceEndPoint, AllDataSources);
            }

            // 2) 单 datasource 占位端点:GET api/datasource/{name}
            //    (当前永远不可达,见 Caveat)
            var template = options.DataSourceEndpointTemplate;
            if (!string.IsNullOrWhiteSpace(template))
            {
                var dataSourceApiPath = string.Format(template, "{name}");
                endpoints.MapGet(dataSourceApiPath, LoadDataSource);
            }

            PagedList<EmptyDataSource> LoadDataSource(
                string name,
                int? offset,
                int? limit,
                string? filter,
                string? agg,
                bool? countAll,
                string? orderBy,
                string? select,
                bool? distinct)
            {
                throw new Exception($"Datasource '{name}' can not found.");
            }
            Dictionary<string, DataSourceInfo> AllDataSources(IServiceProvider sp)
            {
                var options = sp.GetRequiredService<IOptions<DataSourceOptions>>().Value;
                return options.DataSources;
            }
        }
    }

    /// <summary>
    /// 占位类型:fake datasource endpoint 当前返回的 PagedList 元素类型。
    /// 真实场景下,这个 endpoint 应该返回对应 datasource 的"形状信息"(字段、类型等),
    /// 或者在去掉重写后真正派发到具体 controller 取数据。
    /// </summary>
    internal record EmptyDataSource;
}

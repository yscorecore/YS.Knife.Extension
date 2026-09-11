using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using YS.Knife.Query;

namespace YS.Knife.DataSource.AspnetCore
{
    [Options]
    public record class DataSourceOptions
    {

        public string DataSourceEndpointTemplate { get; set; } = "api/datasource/{0}/load-data";


        public string AllDataSourceEndPoint { get; set; } = "api/datasource/all";


        public bool MapAllDataSourceEndPoint { get; set; } = true;


        public Dictionary<string, DataSourceInfo> DataSources { get; set; } = new();
    }
    public static class DataSourceOptionsExtensions
    {
        public static DataSourceOptions AddDataSource(this DataSourceOptions options, string name, string endPointPath, string? description = null, string? filter = null)
        {
            ArgumentNullException.ThrowIfNull(options);
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("name can not be null or whitespace.", nameof(name));
            }
            if (string.IsNullOrWhiteSpace(endPointPath))
            {
                throw new ArgumentException("endPointPath can not be null or whitespace.", nameof(endPointPath));
            }
            if (options.DataSources == null)
            {
                options.DataSources = new Dictionary<string, DataSourceInfo>();
            }
            options.DataSources[name] = new DataSourceInfo
            {
                EndPointPath = endPointPath,
                Description = description,
                Filter = filter
            };
            return options;
        }

        public static DataSourceOptions AddDataSource(this DataSourceOptions options, string name, DataSourceInfo info)
        {
            ArgumentNullException.ThrowIfNull(options);
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("name can not be null or whitespace.", nameof(name));
            }
            if (info is null || string.IsNullOrWhiteSpace(info.EndPointPath))
            {
                throw new ArgumentException("info can not be null and info.EndPointPath can not be null or whitespace.", nameof(info));
            }
            if (options.DataSources == null)
            {
                options.DataSources = new Dictionary<string, DataSourceInfo>();
            }
            options.DataSources[name] = info;
            return options;
        }

        public static DataSourceOptions AddDataSource<TController, T>(this DataSourceOptions options, string name, Expression<Func<TController, Task<PagedList<T>>>> exp, Expression<Func<T, bool>>? filter = default)
           where TController : ControllerBase
        {
            var endPointInfo = GetEndPointInfo<TController>(exp);
            return options.AddDataSource(name, endPointInfo.EndPointPath, endPointInfo.Description, filter != null ? YS.Knife.Query.Builder.CreateFilter(filter) : null);
        }
        public static DataSourceOptions AddDataSource<TController, T>(this DataSourceOptions options, string name, Expression<Func<TController, ValueTask<PagedList<T>>>> exp, Expression<Func<T, bool>>? filter = default)
          where TController : ControllerBase
        {
            var endPointInfo = GetEndPointInfo<TController>(exp);
            return options.AddDataSource(name, endPointInfo.EndPointPath, endPointInfo.Description, filter != null ? YS.Knife.Query.Builder.CreateFilter(filter) : null);
        }
        private static EndPointInfo GetEndPointInfo<TController>(LambdaExpression exp)
        {
            ArgumentNullException.ThrowIfNull(exp);

            var controllerType = typeof(TController);
            var action = ExtractActionMethod(exp, controllerType);

            var controllerPath = ResolveControllerPath(controllerType);
            var actionPath = ResolveActionPath(action, controllerType);

            // 拼接时过滤空片段,避免裸 [HttpGet] 产生 "products/" 这样的尾斜杠
            var endPointPath = string.Join('/',
                new[] { controllerPath, actionPath }.Where(segment => !string.IsNullOrWhiteSpace(segment)));

            return new EndPointInfo
            {
                EndPointPath = endPointPath,
                Description = action.GetCustomAttribute<DescriptionAttribute>()?.Description,
            };
        }


        private static MethodInfo ExtractActionMethod(LambdaExpression exp, Type controllerType)
        {
            Expression body = exp.Body;
            while (body is UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked } unary)
            {
                body = unary.Operand;
            }

            if (body is not MethodCallExpression call)
            {
                throw new ArgumentException(
                    $"Expression body must be a direct method call on {controllerType.Name}, e.g. c => c.Action(...).",
                    nameof(exp));
            }

            return call.Method;
        }


        private static string ResolveControllerPath(Type controllerType)
        {
            var template = controllerType
                .GetCustomAttributes<RouteAttribute>(inherit: true)
                .Select(attribute => attribute.Template)
                .FirstOrDefault(t => !string.IsNullOrWhiteSpace(t))
                ?? "[controller]";

            return ReplaceRouteTokens(template!, controllerType, actionName: null).Trim('/');
        }


        private static string ResolveActionPath(MethodInfo action, Type controllerType)
        {
            var httpAttribute = action
                .GetCustomAttributes<HttpMethodAttribute>(inherit: true)
                .FirstOrDefault();

            // 没有 HTTP verb attribute -> 按 MVC 约定以 action 名作为路由段;
            // 裸 [HttpGet] / [HttpPost] 的 Template 为 null -> 该段为空,路径就是 controller 前缀
            var template = httpAttribute is null
                ? "[action]"
                : httpAttribute.Template ?? string.Empty;

            return ReplaceRouteTokens(template, controllerType, action.Name).Trim('/');
        }

        private static string ReplaceRouteTokens(string template, Type controllerType, string? actionName)
        {
            if (string.IsNullOrWhiteSpace(template))
            {
                return string.Empty;
            }

            var controllerName = controllerType.Name.EndsWith("Controller", StringComparison.Ordinal)
                ? controllerType.Name[..^"Controller".Length]
                : controllerType.Name;

            return Regex.Replace(
                template,
                @"\[(controller|action|area)\]",
                match => match.Groups[1].Value.ToLowerInvariant() switch
                {
                    "controller" => controllerName,
                    "action" => actionName ?? string.Empty,
                    "area" => string.Empty,
                    _ => match.Value,
                },
                RegexOptions.IgnoreCase);
        }

        private class EndPointInfo
        {
            public string EndPointPath { get; set; } = null!;
            public string? Description { get; set; }
        }
    }
    public record DataSourceInfo
    {
        public string EndPointPath { get; set; } = null!;
        public string? Description { get; set; }

        public string? Filter { get; set; }
    }
    public sealed class It
    {
        public static T Any<T>() => default!;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using YS.Knife.Mapper;
using YS.Knife.Mapper.Impl.FlyTiger;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static void AddFlyTigerMapper(this IServiceCollection services, params Assembly[] assemblies)
        {
            services.AddFlyTigerCoreService();
            services.Configure<FlyTigerMapperOptions>(t =>
            {
                ConfigAssemblyQueryable(t, assemblies[0]);
            });


        }
        private static void ConfigAssemblyQueryable(FlyTigerMapperOptions options, Assembly assembly)
        {
            var extensionType = assembly.GetType("FlyTiger.MapperExtensions", true)!;
            var mapperMethodType = assembly.GetType("FlyTiger.MapperMethodAttribute", true)!;
            foreach (var method in extensionType.GetMethods(BindingFlags.Static | BindingFlags.NonPublic))
            {
                var cm = method.GetCustomAttributesData().Where(p => p.AttributeType == mapperMethodType)
                    .FirstOrDefault();
                if (cm == null) continue;
                var source = (cm.ConstructorArguments[0].Value as Type)!;
                var target = (cm.ConstructorArguments[1].Value as Type)!;
                var type = (cm.ConstructorArguments[2].Value as string)!;
                if (type == "Queryable")
                {
                    var queryablefuncType = typeof(Func<,>).MakeGenericType(typeof(IQueryable<>).MakeGenericType(source),
                        typeof(IQueryable<>).MakeGenericType(target));
                    options.QueryableFuncs.Add((source, target), Delegate.CreateDelegate(queryablefuncType, null, method));
                }
                else if (type == "Convert")
                {
                    var convertfuncType = typeof(Func<,>).MakeGenericType(source, target);
                    options.ConvertFuncs.Add((source, target), Delegate.CreateDelegate(convertfuncType, null, method));
                }
                else if (type == "CopySingle")
                {
                    var copyfuncType = typeof(Action<,,,>).MakeGenericType(source, target,typeof(Action<object>),typeof(Action<object>));
                    options.CopyFuncs.Add((source, target), Delegate.CreateDelegate(copyfuncType, null, method));
                }
            }

        }
        private static void AddFlyTigerCoreService(this IServiceCollection services)
        {
            services.AddSingleton<FlytigerMapper>();
            services.AddSingleton<IQuerableMapper>(sp => sp.GetRequiredService<FlytigerMapper>());
            services.AddSingleton<IConvertMapper>(sp => sp.GetRequiredService<FlytigerMapper>());
            services.AddSingleton<ICopyMapper>(sp => sp.GetRequiredService<FlytigerMapper>());
            services.AddOptions<FlyTigerMapperOptions>();
        }
    }
}

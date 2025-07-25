using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using YS.Knife.Query;

namespace YS.Knife.DataSource.Core.Impl
{
    internal class ServiceRegister : IServiceRegister
    {
        public void RegisterServices(IServiceCollection services, IRegisterContext context)
        {
            RegisterDataSources(services, context);
            RegisterDataMetas(services, context);
        }

        private void RegisterDataSources(IServiceCollection services, IRegisterContext context)
        {
            services.AddOptions<DataSourceOptions>().Configure((o) =>
            {
                var methods = AppDomain.CurrentDomain.GetAssemblies()
                   .Where(p => !p.IsFromMicrosoft())
                   .SelectMany(p => p.GetTypes().Where(t => t.IsInterface))
                   .SelectMany(p => p.GetMethods().Where(t => Attribute.IsDefined(t, typeof(DataSourceAttribute))));
                foreach (var method in methods)
                {
                    var name = method.GetCustomAttribute<DataSourceAttribute>().Name;
                    CheckDataSourceMethodSignture(method);
                    o.Add(name, (sp, limit) =>
                    {
                        var service = sp.GetRequiredService(method.DeclaringType);
                        return InvokeMethod(method, service, limit);
                    });
                }
            });
        }
        private void RegisterDataMetas(IServiceCollection services, IRegisterContext context)
        {
            //services.AddOptions<DataMetaOptions>().Configure((o) =>
            //{
            //    var metaClasses = AppDomain.CurrentDomain.GetAssemblies()
            //       .Where(p => !p.IsFromMicrosoft())
            //       .SelectMany(p => p.GetTypes().Where(t => t.IsClass && !t.IsAbstract && Attribute.IsDefined(t, typeof(DataMetaAttribute))));
            //    foreach (var metaClass in metaClasses)
            //    {
            //        var attr = metaClass.GetCustomAttribute<DataMetaAttribute>();
            //        var name = string.IsNullOrEmpty(attr?.Name) ? metaClass.Name : attr.Name;
            //        o.Add(name, (sp) => sp.GetRequiredService<IDataMetaFactory>().GetDataMetaForType(metaClass));
            //    }
            //    //注册datasource的数据源的元数据
            //    var methods = AppDomain.CurrentDomain.GetAssemblies()
            //      .Where(p => !p.IsFromMicrosoft())
            //      .SelectMany(p => p.GetTypes().Where(t => t.IsInterface))
            //      .SelectMany(p => p.GetMethods().Where(t => Attribute.IsDefined(t, typeof(DataSourceAttribute))));
            //    foreach (var method in methods)
            //    {
            //        var attr = method.GetCustomAttribute<DataSourceAttribute>();
            //        if (attr.AutoRegisterMeta)
            //        {
            //            o.Add(attr.Name, (sp) => sp.GetRequiredService<IDataMetaFactory>().GetDataMetaForType(GetDataSourceClassType(method)));
            //        }
            //    }
            //});
        }
        private void CheckDataSourceMethodSignture(MethodInfo methodInfo)
        {
            try
            {
                if (CheckDataSourceArgumentType() == false || CheckDataSourceReturnType() == false)
                {
                    throw new InvalidOperationException(BuildInvalidMethodErrorMessage(methodInfo));
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(BuildInvalidMethodErrorMessage(methodInfo), ex);
            }
            bool CheckDataSourceArgumentType()
            {
                return methodInfo.GetParameters().Single().ParameterType == typeof(LimitQueryInfo);
            }
            bool CheckDataSourceReturnType()
            {
                var (tDefine, tArg) = DecodeGeneric1Argument(methodInfo.ReturnType);
                return (tDefine == typeof(Task<>) || tDefine == typeof(ValueTask<>)) &&
                    tArg.IsGenericType &&
                    tArg.GetGenericTypeDefinition() == typeof(PagedList<>);
            }
        }
        private string BuildInvalidMethodErrorMessage(MethodInfo methodInfo)
        {
            return $"The data source method signature is invalid.\n" +
             $"The invalid method signature is: '{methodInfo.DeclaringType.FullName}.{methodInfo.Name}'.\n" +
             $"Correct signature should be 'Task<PagedList<T>>(LimitQueryInfo)'.";
        }

        private (Type Define, Type Argument) DecodeGeneric1Argument(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() != null)
            {
                return (type.GetGenericTypeDefinition(), type.GetGenericArguments().Single());
            }
            else
            {
                return (default(Type), default(Type));
            }
        }
        private Type GetDataSourceClassType(MethodInfo methodInfo)
        {
            try
            {
                var (_, tArg) = DecodeGeneric1Argument(methodInfo.ReturnType);
                (_, tArg) = DecodeGeneric1Argument(tArg);
                return tArg;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(BuildInvalidMethodErrorMessage(methodInfo), ex);
            }
        }

        private Task<IPagedList> InvokeMethod(MethodInfo method, object instance, LimitQueryInfo limit)
        {
            var (defined, arg) = DecodeGeneric1Argument(method.ReturnType);
            var (_, metaArg) = DecodeGeneric1Argument(arg);
            ITaskInvoker invoker = Activator.CreateInstance(typeof(TaskInvoker<>).MakeGenericType(metaArg)) as ITaskInvoker;
            if (defined == typeof(ValueTask<>))
            {
                return invoker.InvokeValueTaskMethod(method, instance, limit);
            }
            else
            {
                return invoker.InvokeTaskMethod(method, instance, limit);
            }
        }
        interface ITaskInvoker
        {
            Task<IPagedList> InvokeValueTaskMethod(MethodInfo method, object service, LimitQueryInfo limit);
            Task<IPagedList> InvokeTaskMethod(MethodInfo method, object service, LimitQueryInfo limit);
        }
        class TaskInvoker<T> : ITaskInvoker
        {
            public async Task<IPagedList> InvokeTaskMethod(MethodInfo method, object service, LimitQueryInfo limit)
            {
                var task = (Task<PagedList<T>>)(method.Invoke(service, new object[] { limit }));
                var res = await task;
                return res;
            }

            public async Task<IPagedList> InvokeValueTaskMethod(MethodInfo method, object service, LimitQueryInfo limit)
            {
                var task = (ValueTask<PagedList<T>>)(method.Invoke(service, new object[] { limit }));
                var res = await task;
                return res;
            }
        }
    }
}

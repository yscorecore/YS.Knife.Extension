using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using YS.Knife.Metadata;

namespace YS.Knife.Task.Impl.Default
{
    internal class ServiceRegister : IServiceRegister
    {
        public void RegisterServices(IServiceCollection services, IRegisterContext context)
        {
            var taskTypes = GetTaskTypes(services, context).ToArray();
            var argTypes = GetArgumentTypes(taskTypes).ToArray();
            Array.ForEach(taskTypes, p =>
            {
                services.AddScoped(typeof(ITask), p);
            });
            services.Configure<MetadataOptions>(p =>
            {
                p.AddOrReplaceMeta(argTypes);
            });

        }
        private IEnumerable<Type> GetTaskTypes(IServiceCollection services, IRegisterContext context)
        {
            return AppDomain.CurrentDomain.FindInstanceTypesByAttributeAndBaseType<TaskAttribute, ITask>();
        }
        private IEnumerable<Type> GetArgumentTypes(Type[] taskTypes)
        {
            foreach (var type in taskTypes)
            {
                var baseType = type.BaseType;
                if (baseType == null) continue;
                if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(BaseTask<>))
                {
                    var argType = baseType.GetGenericArguments().Single();
                    yield return argType;
                }
            }
        }
    }
}

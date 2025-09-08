using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace YS.Knife.DataSource
{
    [SingletonPattern]
    public partial class AssemblyDataSourceEntryFinder
    {
        private Lazy<IReadOnlyDictionary<string, DataSourceEntry>> dataSourceEntries = new Lazy<IReadOnlyDictionary<string, DataSourceEntry>>(() => FindAppDomainDataSourceEntries().ToImmutableDictionary(p => p.Name), true);
        public IReadOnlyDictionary<string, DataSourceEntry> All
        {
            get
            {
                return dataSourceEntries.Value;
            }
        }

        public static IEnumerable<DataSourceEntry> FindAppDomainDataSourceEntries()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                    .Where(assembly =>
                    {
                        return !assembly.FullName.StartsWith("System", StringComparison.OrdinalIgnoreCase)
                                && !assembly.FullName.StartsWith("Microsoft", StringComparison.OrdinalIgnoreCase);
                    })
                    .SelectMany(p => p.GetTypes().Where(t => t.IsInterface))
                    .SelectMany(p => p.GetMethods().Where(t => Attribute.IsDefined(t, typeof(DataSourceAttribute))))
                    .SelectMany(p => p.GetCustomAttributes<DataSourceAttribute>().Select(a => new DataSourceEntry
                    {
                        Method = p,
                        Name = a.Name,
                        ServiceType = p.DeclaringType,
                        Arguments = a.Arguments,
                        EntityType = GetDataSourceClassType(p),
                        AutoRegisterMeta = a.AutoRegisterMeta,
                        HasCancellationToken = p.GetParameters().TakeLast(1).Any(q => q.ParameterType == typeof(CancellationToken)),
                        IsValueTask = p.ReturnType.IsGenericType && p.ReturnType.GetGenericTypeDefinition() == typeof(ValueTask<>),
                    }));

        }
        private static (Type Define, Type Argument) DecodeGeneric1Argument(Type type)
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
        private static Type GetDataSourceClassType(MethodInfo methodInfo)
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
        private static string BuildInvalidMethodErrorMessage(MethodInfo methodInfo)
        {
            return $"The data source method signature is invalid.\n" +
             $"The invalid method signature is: '{methodInfo.DeclaringType.FullName}.{methodInfo.Name}'.\n" +
             $"Correct signature should be 'Task<PagedList<T>>(LimitQueryInfo)'.";
        }

    }
}

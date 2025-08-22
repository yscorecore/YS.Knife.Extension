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
    public partial class AssemblyDataItemEntryFinder
    {
        private Lazy<IReadOnlyDictionary<string, DataItemEntry>> DataItemEntries = new Lazy<IReadOnlyDictionary<string, DataItemEntry>>(() => FindAppDomainDataItemEntries().ToImmutableDictionary(p => p.Name), true);
        public IReadOnlyDictionary<string, DataItemEntry> All
        {
            get
            {
                return DataItemEntries.Value;
            }
        }

        public static IEnumerable<DataItemEntry> FindAppDomainDataItemEntries()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                    .Where(assembly =>
                    {
                        return !assembly.FullName.StartsWith("System", StringComparison.OrdinalIgnoreCase)
                                && !assembly.FullName.StartsWith("Microsoft", StringComparison.OrdinalIgnoreCase);
                    })
                    .SelectMany(p => p.GetTypes().Where(t => t.IsInterface))
                    .SelectMany(p => p.GetMethods().Where(t => Attribute.IsDefined(t, typeof(DataItemAttribute))))
                    .SelectMany(p => p.GetCustomAttributes<DataItemAttribute>().Select(a =>
                    {
                        return new DataItemEntry
                        {
                            Method = p,
                            Name = a.Name,
                            ServiceType = p.DeclaringType,
                            ReturnType = GetDataItemClassType(p),
                            AutoRegisterMeta = a.AutoRegisterMeta,
                            Parameters = p.GetParameters().Where(t => t.ParameterType != typeof(CancellationToken)).ToArray(),
                            HasCancellationToken = p.GetParameters().TakeLast(1).Any(q => q.ParameterType == typeof(CancellationToken)),
                            IsValueTask = p.ReturnType.IsGenericType && p.ReturnType.GetGenericTypeDefinition() == typeof(ValueTask<>),
                        };
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
        private static Type GetDataItemClassType(MethodInfo methodInfo)
        {
            try
            {
                var (_, tArg) = DecodeGeneric1Argument(methodInfo.ReturnType);
                return tArg;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(BuildInvalidMethodErrorMessage(methodInfo), ex);
            }
        }
        private static string BuildInvalidMethodErrorMessage(MethodInfo methodInfo)
        {
            return $"The data item method signature is invalid.\n" +
             $"The invalid method signature is: '{methodInfo.DeclaringType.FullName}.{methodInfo.Name}'.\n" +
             $"Correct signature should be 'Task<T>()'.";
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace YS.Knife
{
    public static class ReflectionExtensions
    {
        public static IEnumerable<Type> FindInstanceTypesByAttribute<T>(this AppDomain appDomain,
            Func<Type, bool> filter = null)
            where T : Attribute
        {
            _ = appDomain ?? throw new ArgumentNullException(nameof(appDomain));
            var customFilter = filter ?? (type => true);

            foreach (var assembly in appDomain.GetAllAssembliesIgnoreSystem())
            {
                foreach (var type in assembly.GetLoadableTypes())
                {
                    if (type.IsClass
                        && !type.IsAbstract
                        && Attribute.IsDefined(type, typeof(T), false)
                        && customFilter(type))
                    {
                        yield return type;
                    }
                }
            }
        }

        public static IEnumerable<Type> FindInstanceTypesByBaseType<TBase>(this AppDomain appDomain,
            Func<Type, bool> filter = null)
        {
            _ = appDomain ?? throw new ArgumentNullException(nameof(appDomain));
            var customFilter = filter ?? (type => true);
            foreach (var assembly in appDomain.GetAllAssembliesIgnoreSystem())
            {
                foreach (var type in assembly.GetLoadableTypes())
                {
                    if (type.IsClass
                        && !type.IsAbstract
                        && typeof(TBase).IsAssignableFrom(type)
                        && customFilter(type))
                    {
                        yield return type;
                    }
                }
            }
        }

        public static IEnumerable<Type> FindInstanceTypesByAttributeAndBaseType<TAttrbute, TBase>(
            this AppDomain appDomain, Func<Type, bool> filter = null)
            where TAttrbute : Attribute
        {
            _ = appDomain ?? throw new ArgumentNullException(nameof(appDomain));
            var customFilter = filter ?? (type => true);
            foreach (var assembly in appDomain.GetAllAssembliesIgnoreSystem())
            {
                foreach (var type in assembly.GetLoadableTypes())
                {
                    if (type.IsClass
                        && !type.IsAbstract
                        && Attribute.IsDefined(type, typeof(TAttrbute), false)
                        && typeof(TBase).IsAssignableFrom(type)
                        && customFilter(type))
                    {
                        yield return type;
                    }
                }
            }
        }

        public static bool IsFromMicrosoft(this Assembly assembly)
        {
            var companyAttr = assembly.GetCustomAttribute<AssemblyCompanyAttribute>();
            return companyAttr?.Company == "Microsoft Corporation";
        }

        private static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                // 某些程序集可能引用了缺失的可选依赖（例如 NPOI 的 SkiaSharp 被标记为
                // exclude=Runtime），导致 GetTypes() 抛异常；这里跳过无法加载的类型，
                // 避免整次扫描崩溃。
                return ex.Types.Where(type => type != null);
            }
        }

        private static IEnumerable<Assembly> GetAllAssembliesIgnoreSystem(this AppDomain appDomain)
        {
            var entryAssembly = Assembly.GetEntryAssembly();
            foreach (var assembly in appDomain.GetAssemblies())
            {
                if (assembly != entryAssembly && !IsFromMicrosoft(assembly))
                {
                    yield return assembly;
                }
            }

            yield return entryAssembly;
        }
    }
}

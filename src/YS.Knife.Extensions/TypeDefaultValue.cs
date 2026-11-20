using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace System
{

    public static partial class TypeExtensions
    {
        public static object GetDefaultValue(this Type type)
        {
            return TypeDefaultValue.Instance.GetDefaultValue(type);
        }

        [SingletonPattern]
        partial class TypeDefaultValue
        {
            private static ConcurrentDictionary<Type, object> caches = new ConcurrentDictionary<Type, object>();
            private static MethodInfo genericMethod = typeof(TypeDefaultValue).GetMethod(nameof(GetDefault), BindingFlags.NonPublic | BindingFlags.Static);
            private static T GetDefault<T>()
            {
                return default(T);
            }
            public object GetDefaultValue(Type type)
            {
                return caches.GetOrAdd(type, (t) => genericMethod.MakeGenericMethod(t).Invoke(null, null));
            }
        }
    }

}

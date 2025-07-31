namespace YS.Knife.AspnetCore
{
    internal static class ExceptionExtensions
    {
        private static System.Collections.Concurrent.ConcurrentDictionary<System.Type, Func<Exception, string>>
            _codeCache = new System.Collections.Concurrent.ConcurrentDictionary<Type, Func<Exception, string>>();
        public static bool IsCodeException<T>(this T exception, out string code)
            where T : Exception
        {
            var func = _codeCache.GetOrAdd(typeof(T), (t) => GetCodeFunc(t));
            if (func != null)
            {
                code = func(exception);
                return true;
            }
            else
            {
                code = null;
                return false;
            }
        }
        private static Func<Exception, string> GetCodeFunc(Type type)
        {
            var codeProperty = type.GetProperty("Code", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (codeProperty != null)
            {
                return (exception =>
                {
                    var value = codeProperty.GetValue(exception);
                    return value != null ? value.ToString() : null;
                });
            }
            else
            {
                return null;
            }

        }

    }
}

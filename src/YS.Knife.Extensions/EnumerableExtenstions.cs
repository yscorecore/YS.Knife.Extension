using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace System.Linq
{
    public static class EnumerableExtenstions
    {
        public static IEnumerable<T> OrderBySequence<T>(this IEnumerable<T> source, IEnumerable<T> sortData)
        {
            return source.OrderBySequence(p => p, sortData);
        }
        public static IEnumerable<T> OrderBySequence<T, V>(this IEnumerable<T> source, Func<T, V> sortValue, IEnumerable<V> sortData)
        {
            var map = new Dictionary<V, int>();
            int index = 0;
            foreach (var item in sortData)
            {
                map[item] = index++;
            }
            return source.Select(p =>
            {
                var val = sortValue.Invoke(p);
                var weight = map.TryGetValue(val, out int v) ? v : int.MaxValue;
                return (p, weight);
            }).OrderBy(p => p.weight).Select(p => p.p);
        }
        public static bool HasDuplicate<T>(this IEnumerable<T> source)
        {
            _ = source ?? throw new ArgumentNullException(nameof(source));
            HashSet<T> items = new HashSet<T>();
            foreach (var item in source)
            {
                if (items.Contains(item))
                {
                    return true;
                }
                else
                {
                    items.Add(item);
                }
            }
            return false;
        }
        public static bool HasDuplicate<T, S>(this IEnumerable<T> source, Func<T, S> selector)
        {
            _ = source ?? throw new ArgumentNullException(nameof(source));
            _ = selector ?? throw new ArgumentNullException(nameof(selector));
            HashSet<S> items = new HashSet<S>();
            foreach (var item in source)
            {
                var key = selector(item);
                if (items.Contains(key))
                {
                    return true;
                }
                else
                {
                    items.Add(key);
                }
            }
            return false;
        }

    }

}

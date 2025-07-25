using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using YS.Knife.Entity;

namespace Microsoft.EntityFrameworkCore
{
    [CodeExceptions]
    public static partial class QueryableExtensions
    {
        private static System.Collections.Concurrent.ConcurrentDictionary<Type, string> DisplayNameCaches =
            new System.Collections.Concurrent.ConcurrentDictionary<Type, string>();

        [CodeException("001", "Can not find {name} by id '{id}'.")]
        internal static partial Exception EntityNotFound(string name, object id, CancellationToken token = default);

        [CodeException("002", "Already exists '{name}'.")]
        internal static partial Exception EntityAlreadyExists(string name, object id, CancellationToken token = default);


        public static T FindOrThrow<T, TKey>(this IQueryable<T> source, TKey id)
            where T : IEntity<TKey>
            where TKey : notnull
        {
            return source.Where(p => object.Equals(id, p.Id)).FirstOrDefault() ?? throw EntityNotFound(GetEntityName(typeof(T)), id);
        }
        public static async Task<T> FindOrThrowAsync<T, TKey>(this IQueryable<T> source, TKey id, CancellationToken token = default)
           where T : IEntity<TKey>
            where TKey : notnull
        {
            return (await source.Where(p => object.Equals(id, p.Id)).FirstOrDefaultAsync(token)) ?? throw EntityNotFound(GetEntityName(typeof(T)), id);
        }


        public static T[] FindArrayOrThrow<T, TKey>(this IQueryable<T> source, TKey[] ids)
              where T : IEntity<TKey>
              where TKey : notnull
        {
            var dic = source.FindDictionaryOrThrow(ids);
            return ids.Select(id => dic[id]).ToArray();
        }
        public static async Task<T[]> FindArrayOrThrowAsync<T, TKey>(this IQueryable<T> source, TKey[] ids, CancellationToken token = default)
              where T : IEntity<TKey>
            where TKey : notnull
        {
            var dic = await source.FindDictionaryOrThrowAsync(ids, token);
            return ids.Select(id => dic[id]).ToArray();
        }

        public static IDictionary<TKey, T> FindDictionaryOrThrow<T, TKey>(this IQueryable<T> source, TKey[] ids)
             where T : IEntity<TKey>
             where TKey : notnull
        {
            var res = source.Where(p => ids.Contains(p.Id)).ToDictionary(p => p.Id);
            Array.ForEach(ids, id =>
            {
                if (!res.ContainsKey(id))
                {
                    throw EntityNotFound(GetEntityName(typeof(T)), id);
                }
            });
            return res;
        }
        public static async Task<IDictionary<TKey, T>> FindDictionaryOrThrowAsync<T, TKey>(this IQueryable<T> source, TKey[] ids, CancellationToken token = default)
              where T : IEntity<TKey>
             where TKey : notnull
        {
            var res = await source.Where(p => ids.Contains(p.Id)).ToDictionaryAsync(p => p.Id, token);
            Array.ForEach(ids, id =>
            {
                if (!res.ContainsKey(id))
                {
                    throw EntityNotFound(GetEntityName(typeof(T)), id);
                }
            });
            return res;
        }

        public static void CheckDuplicate<T, TValue>(this IQueryable<T> source, Expression<Func<T, TValue>> prop, TValue value)
        {
            var equalsexpression = Expression.Equal(prop.Body, Expression.Constant(value));
            var filter = Expression.Lambda<Func<T, bool>>(equalsexpression, prop.Parameters.ToArray());
            if (source.Where(filter).Any())
            {
                throw EntityAlreadyExists(GetEntityName(typeof(T)), value);
            }
        }

        private static string GetEntityName(Type type)
        {
            return DisplayNameCaches.GetOrAdd(type, (t) =>
            {
                return t.GetCustomAttribute<DisplayAttribute>()?.Name
                ?? t.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName
                ?? t.GetCustomAttribute<CommentAttribute>()?.Comment
                ?? t.Name;
            });
        }

    }
}

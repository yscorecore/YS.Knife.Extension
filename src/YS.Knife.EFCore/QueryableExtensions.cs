using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography;
using FlyTiger;
using YS.Knife.Entity;

namespace Microsoft.EntityFrameworkCore
{
    [CodeExceptions]
    public static partial class QueryableExtensions
    {
        private static System.Collections.Concurrent.ConcurrentDictionary<Type, string> DisplayNameCaches =
            new System.Collections.Concurrent.ConcurrentDictionary<Type, string>();

        [CodeException("001", "Can not find {name} by id '{id}'.")]
        internal static partial Exception EntityNotFound(string name, object id);

        [CodeException("002", "Already exists '{name}' with id '{id}'.")]
        internal static partial Exception EntityAlreadyExists(string name, object id);

        [CodeException("003", "Can not find {name}.")]
        internal static partial Exception EntityNotFound(string name);

        public static T FindOrThrow<T, TKey>(this IQueryable<T> source)
          where T : IEntity<TKey>
          where TKey : notnull
        {
            return source.FirstOrDefault() ?? throw EntityNotFound(GetEntityName(typeof(T)));
        }
        public static async Task<T> FindOrThrowAsync<T>(this IQueryable<T> source, CancellationToken token = default)
        {
            return (await source.FirstOrDefaultAsync(token)) ?? throw EntityNotFound(GetEntityName(typeof(T)));
        }

        public static T FindOrThrow<T, TKey>(this DbSet<T> source, TKey id)
            where T : class, IEntity<TKey>
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

        static MethodInfo GenericMethod = typeof(Enumerable)
                 .GetMethod(nameof(Enumerable.Contains),
                  BindingFlags.Static | BindingFlags.Public,
                  new Type[]
                  {
                    typeof(IEnumerable<>).MakeGenericType(Type.MakeGenericMethodParameter(0)),
                    Type.MakeGenericMethodParameter(0)
                  });

        public static async Task CheckDuplicateAsync<T, TValue>(IQueryable<T> source, Expression<Func<T, TValue>> prop, params TValue[] values)
        {
            var duplicateKey = values.GroupBy(p => p).Where(t => t.Count() > 1)
                .Select(p => p.Key).FirstOrDefault();
            if (!object.Equals(duplicateKey, default(TValue)))
            {
                throw new Exception("输入的数据重复");
            }
           
            var method = GenericMethod!.MakeGenericMethod(typeof(TValue));

            var containsExpression = Expression.Call(method, Expression.Constant(values), prop.Body);
            var filter = Expression.Lambda<Func<T, bool>>(containsExpression, prop.Parameters.ToArray());
            var currents = await source.Where(filter).Select(prop).ToListAsync();
            if (currents.Count > 0)
            {
                throw new Exception("名称重复");
            }
        }
        public static async Task CheckEditDuplicateAsync<T, TId, TValue>(IQueryable<T> source, Expression<Func<T, TValue>> prop, IEnumerable<(TId,TValue)> values)
            where T : IEntity<TId>
            where TId : notnull
        {
            var duplicateKey = values.GroupBy(p => p.Item2).Where(t => t.Count() > 1)
               .Select(p => p.Key).FirstOrDefault();
            if (!object.Equals(duplicateKey, default(TValue)))
            {
                throw new Exception("输入的数据重复");
            }
            var method = GenericMethod!.MakeGenericMethod(typeof(TValue));
            var nameValues = values.Select(p => p.Item2).ToHashSet();
            var containsExpression = Expression.Call(method, Expression.Constant(nameValues), prop.Body);
            var filter = Expression.Lambda<Func<T, bool>>(containsExpression, prop.Parameters.ToArray());

            var newIdValuePair = Expression.New(typeof(IdValuePair<TId, TValue>));

            var idBind = Expression.Bind(typeof(IdValuePair<TId, TValue>).GetProperty(nameof(IdValuePair<string, string>.Id))!,
                Expression.Property(prop.Parameters.Single(), typeof(T).GetProperty(nameof(IEntity<string>.Id))!)
                );
            var valueBind = Expression.Bind(typeof(IdValuePair<TId, TValue>).GetProperty(nameof(IdValuePair<string, string>.Value))!, prop.Body);

            var memberInit = Expression.MemberInit(newIdValuePair, new[] { idBind, valueBind });
            var selector = Expression.Lambda<Func<T, IdValuePair<TId, TValue>>>(memberInit, prop.Parameters);
            var currents = await source.Where(filter).Select(selector).ToListAsync();
            var editIds = values.Select(p => p.Item1).ToHashSet();
            var invalidItems = currents.Where(p => !editIds.Contains(p.Id)).ToList();
            if (invalidItems.Count > 0)
            {
                var first = invalidItems.First();
                throw EntityAlreadyExists(GetEntityName(typeof(T)), first.Id);
            }
        }
        private class IdValuePair<TID, TValue>
        {
            public TID Id { get; set; }
            public TValue Value { get; set; }
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

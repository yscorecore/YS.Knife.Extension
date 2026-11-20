#nullable enable

using System.Collections.Concurrent;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace YS.Knife.EFCore
{
    /// <summary>
    /// 保存前校验实体上标注了 <see cref="IndexAttribute"/> 且 IsUnique 的属性组合不与已有数据冲突，
    /// 冲突时抛出 <see cref="ValidationException"/>。
    /// </summary>
    [Service(typeof(IInterceptor), Lifetime = ServiceLifetime.Singleton)]
    public class UniqueIndexInterceptor : SaveChangesInterceptor
    {
        // CLR 类型 → 该类上所有 IsUnique 的 IndexAttribute
        private static readonly ConcurrentDictionary<Type, IndexAttribute[]> UniqueIndexCache = new();

        // 泛型辅助方法的 MethodInfo（用于反射调用，内部走强类型 EF Core API）
        private static readonly MethodInfo ExistsAsyncMethod = typeof(UniqueIndexInterceptor)
            .GetMethod(nameof(ExistsAsync), BindingFlags.NonPublic | BindingFlags.Static)!;

        private static Task<bool> ExistsAsync<T>(
            DbContext context, Expression<Func<T, bool>> predicate, CancellationToken ct)
            where T : class
            => context.Set<T>().AnyAsync(predicate, ct);

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (eventData.Context is { } context)
                await CheckUniqueAsync(context, cancellationToken);
            return result;
        }

        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            if (eventData.Context is { } context)
                CheckUniqueAsync(context, CancellationToken.None).GetAwaiter().GetResult();
            return result;
        }

        private static async Task CheckUniqueAsync(DbContext context, CancellationToken ct)
        {
            var entries = context.ChangeTracker.Entries()
                .Where(e => e.State is EntityState.Added or EntityState.Modified)
                .ToList();

            foreach (var entry in entries)
            {
                var uniqueIndexes = UniqueIndexCache.GetOrAdd(entry.Metadata.ClrType, static clrType =>
                    clrType.GetCustomAttributes<IndexAttribute>(true)
                        .Where(a => a.IsUnique)
                        .ToArray());

                foreach (var index in uniqueIndexes)
                {
                    // 更新场景：只有唯一索引涉及的字段变化时才查库
                    if (entry.State == EntityState.Modified
                        && !index.PropertyNames.Any(p => entry.Property(p).IsModified))
                    {
                        continue;
                    }

                    await CheckOneAsync(context, entry, index, entries, ct);
                }
            }
        }

        private static async Task CheckOneAsync(
            DbContext context,
            EntityEntry entry,
            IndexAttribute index,
            List<EntityEntry> allEntries,
            CancellationToken ct)
        {
            var entityType = entry.Metadata.ClrType;
            var values = index.PropertyNames.Select(p => entry.Property(p).CurrentValue).ToArray();

            // 任一列为空则不参与唯一性校验（唯一索引通常允许多个 NULL）
            if (values.Any(v => v is null)) return;

            // ---------- 1. 构造数据库端谓词：所有索引列均相等 ----------
            var param = Expression.Parameter(entityType, "e");
            Expression predicate = index.PropertyNames
                .Select((p, i) => BuildEquals(Expression.Property(param, p), values[i]))
                .Aggregate(Expression.AndAlso);

            // 更新时排除自身：NOT(pk1 == v1 AND pk2 == v2 ...)，兼容复合主键
            if (entry.State == EntityState.Modified)
            {
                var pkEquals = BuildPrimaryKeyEquals(entry, param);
                if (pkEquals is not null)
                    predicate = Expression.AndAlso(predicate, Expression.Not(pkEquals));
            }

            var lambda = Expression.Lambda(predicate, param);

            // ---------- 2. 异步查库（软删除由 HasQueryFilter 自动生效） ----------
            var existsInDb = await (Task<bool>)ExistsAsyncMethod
                .MakeGenericMethod(entityType)
                .Invoke(null, new object[] { context, lambda, ct })!;

            // ---------- 3. 检查本批次内冲突（ChangeTracker 里尚未落库的兄弟实体） ----------
            var conflictInBatch = allEntries.Any(other =>
                other != entry
                && other.Metadata.ClrType == entityType
                && other.State is EntityState.Added or EntityState.Modified
                && index.PropertyNames.All(p => Equals(
                    other.Property(p).CurrentValue,
                    entry.Property(p).CurrentValue)));

            if (existsInDb || conflictInBatch)
            {
                var displayName = GetPropertyDisplayName(entityType, index.PropertyNames.Last());
                throw new ValidationException($"{displayName} 重复");
            }
        }

        private static string GetPropertyDisplayName(Type entityType, string propertyName)
        {
            var prop = entityType.GetProperty(propertyName);
            return prop?.GetCustomAttribute<DescriptionAttribute>(true)?.Description
                ?? prop?.GetCustomAttribute<CommentAttribute>(true)?.Comment
                ?? propertyName;
        }

        // 复合主键：拼接 (pk1 == v1 && pk2 == v2 && ...)
        private static Expression? BuildPrimaryKeyEquals(EntityEntry entry, ParameterExpression param)
        {
            var pk = entry.Metadata.FindPrimaryKey();
            if (pk is null || pk.Properties.Count == 0) return null;

            Expression? result = null;
            foreach (var pkProp in pk.Properties)
            {
                var pkValue = entry.Property(pkProp.Name).CurrentValue;
                if (pkValue is null) continue;

                Expression constant = Expression.Constant(pkValue);
                if (constant.Type != pkProp.ClrType)
                    constant = Expression.Convert(constant, pkProp.ClrType);

                var eq = Expression.Equal(Expression.Property(param, pkProp.Name), constant);
                result = result is null ? eq : Expression.AndAlso(result, eq);
            }
            return result;
        }

        private static Expression BuildEquals(Expression property, object value)
        {
            Expression constant = Expression.Constant(value);
            if (constant.Type != property.Type)
            {
                // 处理 int → int? 这类装箱可空
                if (Nullable.GetUnderlyingType(property.Type) == value.GetType())
                    constant = Expression.Convert(constant, property.Type);
            }
            return Expression.Equal(property, constant);
        }
    }
}

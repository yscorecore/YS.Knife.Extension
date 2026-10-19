#nullable enable

using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace YS.Knife.EFCore
{
    [Service(typeof(IInterceptor), Lifetime = ServiceLifetime.Singleton)]
    public class UniqueNameInterceptor : SaveChangesInterceptor
    {
        // CLR 类型 → 该类上所有带 [UniqueName] 的属性
        private static readonly ConcurrentDictionary<Type, (string PropName, UniqueNameAttribute Attr)[]> AttributeCache = new();

        // 泛型辅助方法的 MethodInfo（用于反射调用，内部走强类型 EF Core API）
        private static readonly MethodInfo ExistsAsyncMethod = typeof(UniqueNameInterceptor)
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
                var uniqueProps = AttributeCache.GetOrAdd(entry.Metadata.ClrType, static clrType =>
                    clrType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .Select(p => (p.Name, Attr: p.GetCustomAttribute<UniqueNameAttribute>(true)))
                        .Where(x => x.Attr is not null)
                        .Select(x => (x.Name, x.Attr!))
                        .ToArray());

                foreach (var (propName, attr) in uniqueProps)
                {
                    // 更新场景：只有涉及唯一性判断的字段变化时才查库
                    if (entry.State == EntityState.Modified)
                    {
                        var relevant = new[] { propName }.Concat(attr.ScopeProperties);
                        if (!relevant.Any(p => entry.Property(p).IsModified))
                            continue;
                    }

                    await CheckOneAsync(context, entry, propName, attr, entries, ct);
                }
            }
        }

        private static async Task CheckOneAsync(
            DbContext context,
            EntityEntry entry,
            string propertyName,
            UniqueNameAttribute attr,
            List<EntityEntry> allEntries,
            CancellationToken ct)
        {
            var entityType = entry.Metadata.ClrType;
            var nameValue = entry.Property(propertyName).CurrentValue;

            // 空值不参与唯一性校验（需强制非空请另加 [Required]）
            if (nameValue is null) return;

            // ---------- 1. 构造数据库端谓词 ----------
            var param = Expression.Parameter(entityType, "e");
            Expression predicate = BuildEquals(
                Expression.Property(param, propertyName), nameValue, attr.IgnoreCase);

            foreach (var scopeProp in attr.ScopeProperties)
            {
                var scopeValue = entry.Property(scopeProp).CurrentValue;
                predicate = Expression.AndAlso(
                    predicate,
                    BuildEquals(Expression.Property(param, scopeProp), scopeValue, false));
            }

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
                && NameMatches(other, entry, propertyName, attr)
                && attr.ScopeProperties.All(sp => Equals(
                    other.Property(sp).CurrentValue,
                    entry.Property(sp).CurrentValue)));

            if (existsInDb || conflictInBatch)
            {
                var scopeDesc = attr.ScopeProperties.Length == 0
                    ? "（全局）"
                    : $"（在相同 {string.Join(", ", attr.ScopeProperties)} 范围内）";

                throw new ValidationException(string.Format(
                    attr.ErrorMessage, propertyName, nameValue, scopeDesc));
            }
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

        private static bool NameMatches(
            EntityEntry a, EntityEntry b, string propertyName, UniqueNameAttribute attr)
        {
            var va = a.Property(propertyName).CurrentValue;
            var vb = b.Property(propertyName).CurrentValue;
            if (va is null || vb is null) return false;

            if (attr.IgnoreCase && va is string sa && vb is string sb)
                return string.Equals(sa, sb, StringComparison.OrdinalIgnoreCase);
            return Equals(va, vb);
        }

        private static Expression BuildEquals(Expression property, object? value, bool ignoreCase)
        {
            if (value is null)
                return Expression.Equal(property, Expression.Constant(null, property.Type));

            Expression constant = Expression.Constant(value);
            if (constant.Type != property.Type)
            {
                // 处理 int → int? 这类装箱可空
                if (Nullable.GetUnderlyingType(property.Type) == value.GetType())
                    constant = Expression.Convert(constant, property.Type);
            }

            if (ignoreCase && property.Type == typeof(string) && value is string)
            {
                var toUpper = typeof(string).GetMethod(nameof(string.ToUpper), Type.EmptyTypes)!;
                return Expression.Equal(
                    Expression.Call(property, toUpper),
                    Expression.Call(constant, toUpper));
            }

            return Expression.Equal(property, constant);
        }
    }
}

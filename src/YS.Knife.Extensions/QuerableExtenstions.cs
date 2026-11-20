using System.Linq.Expressions;
using System.Reflection;

namespace System.Linq
{
    public static class QueryableExtenstions
    {
        public static IQueryable<T> WhereIf<T>(this IQueryable<T> query, bool condition, Expression<Func<T, bool>> predicate)
        {
            if (condition)
            {
                return query.Where(predicate);
            }
            return query;
        }
        public static IQueryable<R> WhereItemsAnd<T, R>(this IQueryable<R> query, IEnumerable<T> source, Expression<Func<T, R, bool>> predicate)
        {
            Expression expression = Expression.Constant(true);
            foreach (var item in source)
            {
                var parameterReplacer = new ParameterReplacerVisitor(predicate.Parameters[1], Expression.Constant(item));
                var segment = parameterReplacer.Visit(predicate.Body);
                expression = Expression.AndAlso(expression, segment);
            }
            var lambda = Expression.Lambda<Func<R, bool>>(expression, predicate.Parameters[0]);
            return query.Where(lambda);
        }
        public static IQueryable<R> WhereItemsOr<T, R>(this IQueryable<R> query, IEnumerable<T> source, Expression<Func<R, T, bool>> predicate)
        {
            Expression expression = Expression.Constant(false);
            foreach (var item in source)
            {
                var parameterReplacer = new ParameterReplacerVisitor(predicate.Parameters[1], Expression.Constant(item));
                var segment = parameterReplacer.Visit(predicate.Body);
                expression = Expression.OrElse(expression, segment);
            }
            var lambda = Expression.Lambda<Func<R, bool>>(expression, predicate.Parameters[0]);
            return query.Where(lambda);
        }
        public static T Summary<R, T>(this IQueryable<R> query, Expression<Func<IGrouping<int, R>, T>> expression, T defaultValue = default)
        {
            return query.GroupBy(p => 1).Select(expression).FirstOrDefault() ?? defaultValue;
        }
        public static IQueryable<R> Map<R>(this IQueryable query)
            where R : class, new()
        {
            _ = query ?? throw new ArgumentNullException(nameof(query));

            var sourceType = query.ElementType;
            var destType = typeof(R);

            // 获取源和目标类型的公共实例可读/可写属性
            var sourceProps = sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                        .Where(p => p.CanRead && p.GetMethod != null);
            var destProps = destType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                    .Where(p => p.CanWrite && p.SetMethod != null);

            // 匹配名称和类型完全相同的属性
            var matchedProps = from d in destProps
                               join s in sourceProps on new { d.Name, d.PropertyType } equals new { s.Name, s.PropertyType }
                               select new { Source = s, Destination = d };

            // 创建 Lambda 参数：source
            ParameterExpression sourceParam = Expression.Parameter(sourceType, "source");

            // 创建目标对象的新实例（无参构造函数）
            NewExpression newExpr = Expression.New(destType);

            // 构建成员绑定列表
            List<MemberBinding> bindings = new List<MemberBinding>();
            foreach (var match in matchedProps)
            {
                MemberExpression sourcePropExpr = Expression.Property(sourceParam, match.Source);
                MemberAssignment assignment = Expression.Bind(match.Destination, sourcePropExpr);
                bindings.Add(assignment);
            }

            // 如果没有任何匹配，仍可构造对象（属性均为默认值）
            MemberInitExpression memberInit = Expression.MemberInit(newExpr, bindings);

            // 构建 Lambda: source => new R { Prop1 = source.Prop1, ... }
            LambdaExpression lambda = Expression.Lambda(memberInit, sourceParam);

            // 获取泛型 Queryable.Select 方法
            MethodInfo selectMethod = typeof(Queryable).GetMethods()
                .First(m => m.Name == "Select" && m.GetParameters().Length == 2)
                .MakeGenericMethod(sourceType, destType);

            // 构建调用 Select 的表达式树
            MethodCallExpression selectCall = Expression.Call(
                null,                                   // 静态方法
                selectMethod,
                query.Expression,                       // IQueryable<TSource>
                Expression.Quote(lambda)               // Expression<Func<TSource, R>>
            );

            // 通过 Provider 创建新的 IQueryable<R>
            return query.Provider.CreateQuery<R>(selectCall);
        }

        class ParameterReplacerVisitor : ExpressionVisitor
        {
            private readonly Expression _oldParameter;
            private readonly Expression _newParameter;

            public ParameterReplacerVisitor(Expression oldParameter, Expression newParameter)
            {
                _oldParameter = oldParameter;
                _newParameter = newParameter;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                return node == _oldParameter ? _newParameter : base.VisitParameter(node);
            }
        }
    }

}

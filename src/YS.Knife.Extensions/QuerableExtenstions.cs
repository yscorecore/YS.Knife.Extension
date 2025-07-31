using System.Linq.Expressions;

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

using System.Linq.Expressions;

namespace System.Linq.Expressions
{
    public static class ExpressionExtensions
    {
        class ParameterReplacer : ExpressionVisitor
        {
            private readonly ParameterExpression m_parameter;
            private readonly Expression m_replacement;

            public ParameterReplacer(ParameterExpression parameter, Expression replacement)
            {
                this.m_parameter = parameter;
                this.m_replacement = replacement;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                if (ReferenceEquals(node, m_parameter))
                    return m_replacement;
                return node;
            }
        }

        public static Expression ReplaceFirstParam(this LambdaExpression expression, Expression newParameter)
        {
            var parameterToRemove = expression.Parameters.ElementAt(0);
            var replacer = new ParameterReplacer(parameterToRemove, newParameter);
            return replacer.Visit(expression.Body);
        }

        public static LambdaExpression CombinAndAlso(this IEnumerable<LambdaExpression> source, Type paramType)
        {
            var expressions = source.ToList();
            if (expressions.Any())
            {
                var paramExpression = Expression.Parameter(paramType, "m");
                var result = expressions.First().ReplaceFirstParam(paramExpression);
                for (int i = 1; i < expressions.Count; i++)
                {
                    result = Expression.AndAlso(result, expressions[i].ReplaceFirstParam(paramExpression));
                }
                return Expression.Lambda(result, paramExpression);
            }
            return null;
        }
        public static LambdaExpression CombinOrElse(this IEnumerable<LambdaExpression> source, Type paramType)
        {
            var expressions = source.ToList();
            if (expressions.Any())
            {
                var paramExpression = Expression.Parameter(paramType, "m");
                var result = expressions.First().ReplaceFirstParam(paramExpression);
                for (int i = 1; i < expressions.Count; i++)
                {
                    result = Expression.OrElse(result, expressions[i].ReplaceFirstParam(paramExpression));
                }
                return Expression.Lambda(result, paramExpression);
            }
            return null;
        }
    }
}

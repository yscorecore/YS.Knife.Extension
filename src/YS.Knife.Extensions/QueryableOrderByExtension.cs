using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Linq
{
    public static class QueryableOrderByExtension
    {
        static readonly string[] SkipMethodNames = new string[]
            {
                nameof(Queryable.Where),
                nameof(Queryable.Select),
                nameof(Queryable.Skip),
                nameof(Queryable.SelectMany),
                nameof(Queryable.Take),
                nameof(Queryable.Distinct),
                nameof(Queryable.DistinctBy),
                nameof(Queryable.GroupBy)
            };
        static readonly string[] OrderByMethods = new string[] {
                 nameof(Queryable.OrderBy),
                 nameof(Queryable.OrderByDescending),
                 nameof(Queryable.ThenBy),
                 nameof(Queryable.ThenByDescending),

            };
        public static IQueryable<T> TryOrderByEntityKey<T>(this IQueryable<T> query)
        {
            Stack<MethodCallExpression> methodStacks = new Stack<MethodCallExpression>();
            Expression expression = query.Expression;
            while (expression is MethodCallExpression methodCall && methodCall.Method.DeclaringType == typeof(Queryable) && SkipMethodNames.Contains(methodCall.Method.Name))
            {
                methodStacks.Push(methodCall);
                expression = methodCall.Arguments[0];
            }
            //找到原始的类型
            var itemType = expression.Type.GetGenericArguments()[0];
            var keyProperty = FindKeyProperty(itemType);
            if (keyProperty == null || HasOrderByKey(expression, keyProperty))
            {
                return query;
            }
            else
            {
                var orderByMethod = typeof(IOrderedQueryable<>).MakeGenericType(itemType) == expression.Type ?
                      GetThenByMethod(itemType, keyProperty.PropertyType) : GetOrderByMethod(itemType, keyProperty.PropertyType);

                expression = Expression.Call(null, orderByMethod, expression, BuildKeySelect(itemType, keyProperty));
                while (methodStacks.Any())
                {
                    var methodCall = methodStacks.Pop();
                    var arguments = methodCall.Arguments.ToArray();
                    arguments[0] = expression;
                    expression = Expression.Call(null, methodCall.Method, arguments);
                }
                return query.Provider.CreateQuery<T>(expression);
            }

        }
        private static MethodInfo OrderByGenericMethod = typeof(Queryable).GetMethods(BindingFlags.Static | BindingFlags.Public).Where(p => p.Name == nameof(Queryable.OrderBy) && p.GetParameters().Length == 2).SingleOrDefault();
        private static MethodInfo ThenByGenericMethod = typeof(Queryable).GetMethods(BindingFlags.Static | BindingFlags.Public).Where(p => p.Name == nameof(Queryable.ThenBy) && p.GetParameters().Length == 2).SingleOrDefault();

        private static MethodInfo GetOrderByMethod(Type type, Type keyType)
        {
            return OrderByGenericMethod.MakeGenericMethod(type, keyType);
        }
        private static MethodInfo GetThenByMethod(Type type, Type keyType)
        {
            return ThenByGenericMethod.MakeGenericMethod(type, keyType); ;
        }
        private static Expression BuildKeySelect(Type type, PropertyInfo property)
        {
            var p = Expression.Parameter(type, "p");
            return Expression.Quote(Expression.Lambda(Expression.Property(p, property), p));
        }
        private static bool HasOrderByKey(Expression expression, PropertyInfo property)
        {
            while (expression is MethodCallExpression methodCall && methodCall.Method.DeclaringType == typeof(Queryable) && OrderByMethods.Contains(methodCall.Method.Name))
            {
                if (LambdaContainsProperty(methodCall.Arguments[1], property))
                {
                    return true;
                }
                expression = methodCall.Arguments[0];
            }
            return false;
        }
        private static bool LambdaContainsProperty(Expression expression, PropertyInfo property)
        {
            var quoteExpression = expression as UnaryExpression;
            var lambdaExpression = quoteExpression?.Operand as LambdaExpression;
            var lambdaBody = lambdaExpression?.Body as MemberExpression;
            return lambdaBody?.Member == property;
        }
        private static PropertyInfo FindKeyProperty(Type type)
        {
            var allProperties = type.GetProperties();
            return
                allProperties.Where(p => Attribute.IsDefined(p, typeof(KeyAttribute))).FirstOrDefault()
                ?? allProperties.Where(p => p.Name.Equals("Id", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault()
                ?? allProperties.Where(p => p.Name.Equals($"{type.Name}Id", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
        }
    }

}

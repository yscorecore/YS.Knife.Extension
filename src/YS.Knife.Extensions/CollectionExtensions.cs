using System.Linq.Expressions;

namespace YS.Knife
{
    public static class CollectionExtensions
    {
        public static void AppendTo<Source, Target, TKey>(
          this ICollection<Source> source,
          ICollection<Target> target,
          Func<Source, TKey> sourceKey, Func<Target, TKey> targetKey,
          Func<Source, Target> convertFunc, Action<Target> onAddNew = null)
        {
            source.SaveTo(target, CollectionSaveMode.Append, sourceKey, targetKey, convertFunc, null, onAddNew);
        }


        public static void AppendTo<Source, Target, TKey>(
            this ICollection<Source> source,
            ICollection<Target> target,
            Expression<Func<Source, TKey>> sourceKeyExp,
            Func<Source, Target> convertFunc, Action<Target> onAddNew = null)
        {
            source.SaveTo(target, CollectionSaveMode.Append, sourceKeyExp, convertFunc, null, onAddNew);
        }



        public static void MergeTo<Source, Target, TKey>(
            this ICollection<Source> source,
            ICollection<Target> target,
            Func<Source, TKey> sourceKey, Func<Target, TKey> targetKey,
            Func<Source, Target> convertFunc, Action<Source, Target> updateAction, Action<Target> onAddNew = null)
        {
            source.SaveTo(target, CollectionSaveMode.Merge, sourceKey, targetKey, convertFunc, updateAction, onAddNew);
        }
        public static void MergeTo<Source, Target, TKey>(
            this ICollection<Source> source,
            ICollection<Target> target,
            Expression<Func<Source, TKey>> sourceKeyExp,
            Func<Source, Target> convertFunc, Action<Source, Target> updateAction, Action<Target> onAddNew = null)
        {
            source.SaveTo(target, CollectionSaveMode.Merge, sourceKeyExp, convertFunc, updateAction, onAddNew);
        }
        public static void UpdateTo<Source, Target, TKey>(
            this ICollection<Source> source,
            ICollection<Target> target,
            Func<Source, TKey> sourceKey, Func<Target, TKey> targetKey,
            Func<Source, Target> convertFunc, Action<Source, Target> updateAction, Action<Target> onAddNew = null, Action<Target> onRemoveOld = null)
        {
            source.SaveTo(target, CollectionSaveMode.Update, sourceKey, targetKey, convertFunc, updateAction, onAddNew, onRemoveOld);
        }
        public static void UpdateTo<Source, Target, TKey>(
            this ICollection<Source> source,
            ICollection<Target> target,
            Expression<Func<Source, TKey>> sourceKeyExp,
            Func<Source, Target> convertFunc, Action<Source, Target> updateAction, Action<Target> onAddNew = null, Action<Target> onRemoveOld = null)
        {
            source.SaveTo(target, CollectionSaveMode.Update, sourceKeyExp, convertFunc, updateAction, onAddNew, onRemoveOld);
        }

        public static void SaveTo<Source, Target, TKey>(
            this ICollection<Source> source,
            ICollection<Target> target,
            CollectionSaveMode saveMode, Func<Source, TKey> sourceKey, Func<Target, TKey> targetKey,
            Func<Source, Target> convertFunc, Action<Source, Target> updateAction, Action<Target> onAddNew = null, Action<Target> onRemoveOld = null)
        {
            _ = source ?? throw new ArgumentNullException(nameof(source), "Source collection is null");
            _ = target ?? throw new ArgumentNullException(nameof(target), "Target collection is null");
            _ = sourceKey ?? throw new ArgumentNullException(nameof(sourceKey), "Source key function is null");
            _ = targetKey ?? throw new ArgumentNullException(nameof(targetKey), "Target key function is null");
            _ = convertFunc ?? throw new ArgumentNullException(nameof(convertFunc), "Convert function is null");
            if (saveMode >= CollectionSaveMode.Merge)
            {
                _ = updateAction ?? throw new ArgumentNullException(nameof(updateAction), "Update action is null");
            }
            var sourceKV = source.Select(p => new { Key = sourceKey(p), Value = p }).ToList();
            var targetDic = target.ToDictionary(targetKey);
            var allTargetKeys = targetDic.Keys.ToHashSet();

            foreach (var kv in sourceKV.Where(p => !allTargetKeys.Contains(p.Key)))
            {
                var newItem = convertFunc(kv.Value);
                target.Add(newItem);
                onAddNew?.Invoke(newItem);
            }

            if (saveMode >= CollectionSaveMode.Merge)
            {
                foreach (var kv in sourceKV.Where(p => allTargetKeys.Contains(p.Key)))
                {
                    updateAction(kv.Value, targetDic[kv.Key]);
                    targetDic.Remove(kv.Key);
                }
            }
            if (saveMode >= CollectionSaveMode.Update)
            {
                foreach (var kv in targetDic)
                {
                    onRemoveOld?.Invoke(kv.Value);
                    target.Remove(kv.Value);
                }
            }
        }

        public static void SaveTo<Source, Target, TKey>(
            this ICollection<Source> source,
            ICollection<Target> target,
            CollectionSaveMode saveMode, Expression<Func<Source, TKey>> sourceKeyExp,
            Func<Source, Target> convertFunc, Action<Source, Target> updateAction, Action<Target> onAddNew = null, Action<Target> onRemoveOld = null)
        {
            _ = source ?? throw new ArgumentNullException(nameof(source), "Source collection is null");
            _ = target ?? throw new ArgumentNullException(nameof(target), "Target collection is null");
            _ = sourceKeyExp ?? throw new ArgumentNullException(nameof(sourceKeyExp), "Source key expression is null");
            var sourceKeyFunc = GetSourceKeySelectorFunc(sourceKeyExp);
            var targetKeyFunc = GetTargetKeySelectorFunc<Source, Target, TKey>(sourceKeyExp);
            SaveTo(source, target, saveMode, sourceKeyFunc, targetKeyFunc, convertFunc, updateAction, onAddNew, onRemoveOld);
        }

        private static readonly System.Collections.Concurrent.ConcurrentDictionary<(Type Source, string Lambda), Delegate> sourceKeySelectorCache = new System.Collections.Concurrent.ConcurrentDictionary<(Type Source, string Lambda), Delegate>();
        private static Func<Source, Key> GetSourceKeySelectorFunc<Source, Key>(Expression<Func<Source, Key>> sourceKeyLambda)
        {
            var cacheKey = (typeof(Source), sourceKeyLambda.ToString());
            return (Func<Source, Key>)sourceKeySelectorCache.GetOrAdd(cacheKey, (_) =>
            {
                return sourceKeyLambda.Compile();
            });
        }

        private static readonly System.Collections.Concurrent.ConcurrentDictionary<(Type Source, Type Target, string Lambda), Delegate> targetKeySelectorCache = new System.Collections.Concurrent.ConcurrentDictionary<(Type Source, Type Target, string Lambda), Delegate>();
        private static Func<Target, Key> GetTargetKeySelectorFunc<Source, Target, Key>(Expression<Func<Source, Key>> sourceKeyLambda)
        {
            var cacheKey = (typeof(Source), typeof(Target), sourceKeyLambda.ToString());
            return (Func<Target, Key>)targetKeySelectorCache.GetOrAdd(cacheKey, (_) =>
            {
                var newParameter = Expression.Parameter(typeof(Target), "p");
                var replacer = new ParameterExpressionReplacer(newParameter);
                var newLambdaBody = replacer.Visit(sourceKeyLambda.Body);
                var newLambda = Expression.Lambda<Func<Target, Key>>(newLambdaBody, newParameter);
                return newLambda.Compile();
            });
        }
        private class ParameterExpressionReplacer : ExpressionVisitor
        {
            public ParameterExpressionReplacer(ParameterExpression newParameter)
            {
                this.newParameter = newParameter;
            }
            private readonly ParameterExpression newParameter;

            protected override Expression VisitMember(MemberExpression node)
            {
                if (node.Expression.NodeType == ExpressionType.Parameter)
                {
                    return Expression.PropertyOrField(newParameter, node.Member.Name);
                }
                else
                {
                    return base.VisitMember(node);
                }
            }
        }
    }
}

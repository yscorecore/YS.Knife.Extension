using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YS.Knife
{
    public static class ObjectExtensions
    {
        public static void CopyTo<TSource, TTarget>(this TSource source, TTarget target)
            where TTarget : class
        {
            _ = source ?? throw new ArgumentNullException(nameof(source));
            _ = target ?? throw new ArgumentNullException(nameof(target));
            var sourceDic = typeof(TSource).GetProperties().Where(p => p.CanRead && p.IsSpecialName == false).ToDictionary(p => p.Name);
            var targetDic = typeof(TTarget).GetProperties().Where(p => p.CanWrite && p.IsSpecialName == false).ToDictionary(p => p.Name);
            foreach (var p in sourceDic.Keys.Intersect(targetDic.Keys))
            {
                var sourceProp = sourceDic[p];
                var targetProp = targetDic[p];
                if (sourceProp.PropertyType == targetProp.PropertyType)
                {
                    targetProp.SetValue(target, sourceProp.GetValue(source, null));
                }
            }
        }
    }
}

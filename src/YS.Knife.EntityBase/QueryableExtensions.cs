using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YS.Knife.EntityBase;

namespace System.Linq
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> IgnoreDeleted<T>(this IQueryable<T> query)
          where T : ISoftDeleteEntity
        {
            return query.Where(p => p.IsDeleted == false);
        }
        public static IOrderedQueryable<T> SortByOrder<T>(this IQueryable<T> query)
          where T : ISortableEntity
        {
            if (query is IOrderedQueryable<T> orderedQuery)
            {
                return orderedQuery.ThenBy(p=>p.Order);
            }
            else 
            {
                return query.OrderBy(p => p.Order);
            
            }
        }
        public static IOrderedQueryable<T> SortDescByOrder<T>(this IQueryable<T> query)
            where T : ISortableEntity
        {
            if (query is IOrderedQueryable<T> orderedQuery)
            {
                return orderedQuery.ThenByDescending(p => p.Order);
            }
            else
            {
                return query.OrderBy(p => p.Order);
            }
        }
    }
}

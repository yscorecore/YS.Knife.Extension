using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YS.Knife.Entity
{
    public interface ISoftDeleteEntity
    {
        public bool IsDeleted { get; set; }
    }
    public static class ISoftDeleteEntityEntensions
    {
        public static IQueryable<T> FilterDeleted<T>(this IQueryable<T> source)
            where T : ISoftDeleteEntity
        {
            return source.Where(p => p.IsDeleted == false);
        }
    }
}

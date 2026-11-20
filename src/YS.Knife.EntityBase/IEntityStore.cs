using System.Linq;
using System.Threading.Tasks;

namespace YS.Knife.Entity
{
    public interface IEntityStore<T>
    {
        IQueryable<T> Current { get; }
        void Add(T entity);
        void Delete(T entity);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
    public static class IEntityStoreExtensions
    {
        public static void AddRange<T>(this IEntityStore<T> entityStore, IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                entityStore.Add(entity);
            }
        }
        public static void DeleteRange<T>(this IEntityStore<T> entityStore, IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                entityStore.Delete(entity);
            }
        }
    }
}

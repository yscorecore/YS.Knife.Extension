using System.Linq;
using System.Threading.Tasks;

namespace YS.Knife.Entity
{
    public interface IEntityStore<T>
    {
        IQueryable<T> Current { get; }
        void Add(T entity);
        void Delete(T entity);
        Task SaveChangesAsync();
    }
}

using System.Linq;
using System.Threading.Tasks;

namespace YS.Knife.EntityBase
{
    public interface IEntityStore<T>
    {
        IQueryable<T> Current { get; }
        void Add(T entity);
        void Delete(T entity);
        Task SaveChangesAsync();
    }
}

using Microsoft.EntityFrameworkCore;
using YS.Knife.Entity;
using YS.Knife.Service;

namespace YS.Knife.EFCore.Services
{
    [AutoConstructor]
    public partial class DeleteApi<TEntity, TKey> : IDeleteApi<TKey>
        where TKey : notnull
        where TEntity : class, IEntity<TKey>
    {
        private readonly IEntityStore<TEntity> _entityStore;
        public virtual async Task Delete(TKey[] ids, CancellationToken token = default)
        {
            var entitys = await _entityStore.Current.FindArrayOrThrowAsync(ids, token);
            if (typeof(ISoftDeleteEntity).IsAssignableFrom(typeof(TEntity)))
            {
                entitys.OfType<ISoftDeleteEntity>().ToList().ForEach(p => { p.IsDeleted = true; });
            }
            else
            {
                _entityStore.DeleteRange(entitys);
            }
            await _entityStore.SaveChangesAsync(token);
        }
    }


}

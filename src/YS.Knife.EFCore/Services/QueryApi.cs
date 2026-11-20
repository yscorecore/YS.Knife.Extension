using Microsoft.EntityFrameworkCore;
using YS.Knife.Entity;
using YS.Knife.Mapper;
using YS.Knife.Query;
using YS.Knife.Service;

namespace YS.Knife.EFCore.Services
{
    [AutoConstructor]
    public partial class QueryApi<TEntity, TDto> : IQueryPageApi<TDto>
        where TDto : class, new()
        where TEntity : class
    {
        private readonly IEntityStore<TEntity> _entityStore;
        private readonly IQuerableMapper mapper;
        public virtual Task<PagedList<TDto>> QueryPagedList(LimitQueryInfo req, CancellationToken cancellationToken = default)
        {
            var query = _entityStore.Current.AsNoTracking();
            if (typeof(ISoftDeleteEntity).IsAssignableFrom(typeof(TEntity)))
            {
                query = query.Where(e => !((ISoftDeleteEntity)e).IsDeleted);
            }
            if (typeof(ISortableEntity).IsAssignableFrom(typeof(TEntity)))
            {
                query = query.OrderBy(e => ((ISortableEntity)e).Order);
            }
            else if (typeof(ICreationAuditedEntity).IsAssignableFrom(typeof(TEntity)))
            {
                query = query.OrderBy(e => ((ICreationAuditedEntity)e).CreationTime);
            }
            return mapper.MapQuery<TEntity, TDto>(query).QueryPageAsync(req, cancellationToken);
        }
    }


}

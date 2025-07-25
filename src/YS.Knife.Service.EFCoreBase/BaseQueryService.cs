using Microsoft.EntityFrameworkCore;
using YS.Knife.EntityBase;
using YS.Knife.Query;
namespace YS.Knife.Service
{
    public abstract partial class BaseQueryService<TContext, TEntity, TResDto, TKey> : IQueryPageApi<TResDto>
        where TContext : DbContext
        where TEntity : class, IEntity<TKey>
        where TResDto : class, new()
        where TKey : notnull
    {
        protected TContext Context { get; }
        protected virtual IQueryable<TEntity> GetSource()
        {
            return Context.Set<TEntity>();
        }
        public Task<PagedList<TResDto>> QueryPagedList(LimitQueryInfo req, CancellationToken cancellationToken = default)
        {
            return MapperToResult(GetSource()).QueryPageAsync(req);
        }
        protected abstract IQueryable<TResDto> MapperToResult(IQueryable<TEntity> sources);
    }
}

using YS.Knife.EntityBase;
using Microsoft.EntityFrameworkCore;
using YS.Knife.Query;

namespace YS.Knife.Service
{
    [AutoConstructor]
    public abstract partial class BaseCrudService<TContext, TEntity, TCreateDto, TUpdateDto, TResDto, Tkey>
         : BaseCudService<TContext, TEntity, TCreateDto, TUpdateDto, Tkey>,
         IQueryPageApi<TResDto>
         where TCreateDto : class
         where TUpdateDto : class
         where TEntity : class, IEntity<Tkey>
         where TContext : DbContext
         where TResDto : class, new()
         where Tkey : notnull
    {
        public Task<PagedList<TResDto>> QueryPagedList(LimitQueryInfo req, CancellationToken cancellationToken = default)
        {
            return MapperToResult(GetSource()).QueryPageAsync(req);
        }
        protected abstract IQueryable<TResDto> MapperToResult(IQueryable<TEntity> sources);
    }
    [AutoConstructor]
    public abstract partial class BaseCrudService<TContext, TEntity, TDto, Tkey>
        :BaseCrudService<TContext,TEntity,TDto,TDto,TDto,Tkey>
         where TDto : class,new()
         where TEntity : class, IEntity<Tkey>
         where TContext : DbContext
         where Tkey : notnull
    { 
    
    }
}

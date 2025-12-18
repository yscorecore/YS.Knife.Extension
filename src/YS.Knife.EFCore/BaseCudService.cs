using FlyTiger;
using Microsoft.EntityFrameworkCore;
using YS.Knife.Entity;
namespace YS.Knife.Service
{

    [AutoConstructor]
    public abstract partial class BaseCudService<TContext, TEntity, TCreateDto, TUpdateDto, Tkey>
         : ICreateApi<TCreateDto, Tkey>
         , IUpdateApi<TUpdateDto, Tkey>
         , IDeleteApi<Tkey>
         where TCreateDto : class
         where TUpdateDto : class
         where TEntity : class, IEntity<Tkey>
         where TContext : DbContext
         where Tkey : notnull
    {
        protected TContext Context { get; }
        protected virtual IQueryable<TEntity> GetSource()
        {
            return Context.Set<TEntity>();
        }

        public virtual async Task<Tkey[]> Create(TCreateDto[] dtos, CancellationToken token = default)
        {
            var entities = dtos.Select(p => ConvertFromCreateDto(p)).ToList();
            Context.Set<TEntity>().AddRange(entities);
            await Context.SaveChangesAsync();
            return entities.Select(p => p.Id).ToArray();
        }

        public virtual async Task Delete(Tkey[] keys, CancellationToken token = default)
        {
            var entities = await GetSource().FindArrayOrThrowAsync(keys, token);
            Context.Set<TEntity>().RemoveRange(entities);
            await Context.SaveChangesAsync();

        }

        public virtual async Task Update(Tkey[] keys, TUpdateDto dto, CancellationToken token = default)
        {
            var entities = await GetSource().FindArrayOrThrowAsync(keys, token);
            Array.ForEach(entities, p => CopyProperties(dto, p));
            await Context.SaveChangesAsync();

        }
        protected abstract void CopyProperties(TUpdateDto updateDto, TEntity entity);

        protected abstract TEntity ConvertFromCreateDto(TCreateDto createDto);

    }


    [AutoConstructor]
    public abstract partial class BaseCudService<TContext, TEntity, TDto, Tkey>
      : BaseCudService<TContext, TEntity, TDto, TDto, Tkey>
       where TDto : class, new()
       where TEntity : class, IEntity<Tkey>
       where TContext : DbContext
       where Tkey : notnull
    {

    }
}

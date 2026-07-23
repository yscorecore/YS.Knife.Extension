using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using YS.Knife.Entity;
using YS.Knife.Mapper;
using YS.Knife.Operations;
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
            return mapper.MapQuery<TEntity, TDto>(query).QueryPageAsync(req, cancellationToken);
        }
    }
    [AutoConstructor]
    public partial class DeleteApi<TEntity, TKey> : IDeleteApi<TKey>
        where TKey : notnull
        where TEntity : class, IEntity<TKey>
    {
        private readonly IEntityStore<TEntity> _entityStore;
        [Operation(nameof(Delete), "删除{name}")]
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
    [AutoConstructor]
    public partial class CreateApi<TEntity, TCreateDto, TKey> : ICreateApi<TCreateDto, TKey>
        where TCreateDto : class, new()
        where TEntity : class, IEntity<TKey>, new()
    {
        private readonly IEntityStore<TEntity> _entityStore;
        private readonly IConvertMapper mapper;

        [Operation(nameof(Create), "创建{name}")]
        public virtual async Task<TKey[]> Create(TCreateDto[] dtos, CancellationToken token = default)
        {
            var entitys = dtos.Select(p => mapper.Convert<TCreateDto, TEntity>(p)).ToList();
            _entityStore.AddRange(entitys);
            await _entityStore.SaveChangesAsync(token);
            return entitys.Select(p => p.Id).ToArray();
        }
    }

    [AutoConstructor]
    public partial class UpdateApi<TEntity, TUpdateDto, TKey> : IUpdateApi<TUpdateDto, TKey>
       where TUpdateDto : class, new()
       where TEntity : class, IEntity<TKey>, new()
    {
        private readonly IEntityStore<TEntity> _entityStore;
        private readonly ICopyMapper mapper;
        public virtual async Task Update(TKey[] ids, TUpdateDto dto, CancellationToken token = default)
        {
            var entitys = await _entityStore.Current.FindArrayOrThrowAsync(ids, token);
            Array.ForEach(entitys, p => mapper.Copy(dto, p));
            await _entityStore.SaveChangesAsync(token);
        }
    }


}

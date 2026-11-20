using YS.Knife.Entity;
using YS.Knife.Mapper;
using YS.Knife.Service;

namespace YS.Knife.EFCore.Services
{
    [AutoConstructor]
    public partial class CreateApi<TEntity, TCreateDto, TKey> : ICreateApi<TCreateDto, TKey>
        where TCreateDto : class, new()
        where TEntity : class, IEntity<TKey>, new()
    {
        private readonly IEntityStore<TEntity> _entityStore;
        private readonly IConvertMapper mapper;

        public virtual async Task<TKey[]> Create(TCreateDto[] dtos, CancellationToken token = default)
        {
            var entitys = dtos.Select(p => mapper.Convert<TCreateDto, TEntity>(p)).ToList();
            _entityStore.AddRange(entitys);
            await _entityStore.SaveChangesAsync(token);
            return entitys.Select(p => p.Id).ToArray();
        }
    }


}

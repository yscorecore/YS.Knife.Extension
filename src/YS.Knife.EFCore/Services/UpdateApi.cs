using Microsoft.EntityFrameworkCore;
using YS.Knife.Entity;
using YS.Knife.Mapper;
using YS.Knife.Service;

namespace YS.Knife.EFCore.Services
{
    [AutoConstructor]
    public partial class UpdateApi<TEntity, TUpdateDto, TKey> : IUpdateApi<TUpdateDto, TKey>
       where TUpdateDto : class, IIdDto<TKey>, new()
       where TEntity : class, IEntity<TKey>, new()
    {
        private readonly IEntityStore<TEntity> _entityStore;
        private readonly ICopyMapper mapper;


        public async Task Update(TUpdateDto[] dtos, CancellationToken token = default)
        {
            var ids = dtos.Select(p => p.Id).ToArray();
            var enties = await _entityStore.Current.FindDictionaryOrThrowAsync(ids, token);
            var newValueMap = dtos.ToDictionary(p => p.Id);
            foreach (var (k, v) in enties)
            {
                mapper.Copy(newValueMap[k], v);
            }
            await _entityStore.SaveChangesAsync(token);
        }
    }


}

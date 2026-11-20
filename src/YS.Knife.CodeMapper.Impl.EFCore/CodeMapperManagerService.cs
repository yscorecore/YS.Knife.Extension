using Microsoft.EntityFrameworkCore;
using YS.Knife.CodeMapper.Entity.EFCore;
using YS.Knife.Entity;
using YS.Knife.Query;

using static YS.Knife.CodeMapper.ICodeMapperManagerService<System.Guid>;

namespace YS.Knife.CodeMapper.Impl.EFCore
{
    [Service]
    [AutoConstructor]
    [Mapper(typeof(CodeMapperEntity<Guid>), typeof(CodeMapperDto), MapperType = MapperType.Query)]
    [Mapper(typeof(UpdateTargetMapperReq), typeof(CodeMapperEntity<Guid>), MapperType = MapperType.Update)]
    public partial class CodeMapperManagerService : ICodeMapperManagerService<Guid>
    {
        private readonly IEntityStore<CodeMapperEntity<Guid>> codeMapperEntity;
        public Task<PagedList<CodeMapperDto>> QueryPagedList(LimitQueryInfo req, CancellationToken cancellationToken = default)
        {
            return codeMapperEntity.Current.To<CodeMapperDto>().QueryPageAsync(req, cancellationToken);
        }

        public async Task SaveMappers(UpdateTargetMapperReq[] req, CancellationToken cancellationToken = default)
        {
            var all = await codeMapperEntity.Current.FindDictionaryOrThrowAsync(req.Select(t => t.Id).ToArray(), cancellationToken);
            Array.ForEach(req, t => t.To(all[t.Id]));
            await codeMapperEntity.SaveChangesAsync(cancellationToken);
        }
    }
}

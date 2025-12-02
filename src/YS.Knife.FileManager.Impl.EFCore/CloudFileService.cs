using Microsoft.EntityFrameworkCore;
using YS.Knife.Entity;
using YS.Knife.FileManager.Entity.EFCore;
using YS.Knife.LogicRoles;
using YS.Knife.Query;

namespace YS.Knife.FileManager.Impl.EFCore
{
    [AutoConstructor]
    [Service(typeof(ICloudFileService))]
    [Service(typeof(ICloudFileManagerService))]
    [Mapper(typeof(FileEntity<Guid>), typeof(FileDto<Guid>), MapperType = MapperType.Query)]
    [Mapper(typeof(CreateFileDto<Guid>), typeof(FileEntity<Guid>), MapperType = MapperType.Convert)]
    [CodeExceptions(CodePrefix = nameof(CloudFileService))]
    public partial class CloudFileService : ICloudFileService, ICloudFileManagerService
    {
        private readonly IEntityStore<FileEntity<Guid>> entityStore;

        private readonly CloudFileOptions cloudFileOptions;

        private readonly IEnumerable<ILogicRoleProvider> logicRoleProviders;

        [CodeException("001", "只能给目录下面创建文件")]
        private partial Exception ParentIsNotFolder();

        public async Task<Guid[]> Create(CreateFileDto<Guid>[] dtos, CancellationToken token = default)
        {
            var entities = dtos.Select(p => p.To<FileEntity<Guid>>()).ToArray();
            Array.ForEach(entities, p =>
            {
                if (!p.IsFolder)
                {
                    throw ParentIsNotFolder();
                }
                entityStore.Add(p);
            });
            await entityStore.SaveChangesAsync(token);
            return entities.Select(p => p.Id).ToArray();
        }

        public async Task Delete(Guid[] keys, CancellationToken token = default)
        {
            var files = await entityStore.Current.FindArrayOrThrowAsync(keys, token);
            Array.ForEach(files, entityStore.Delete);
            await entityStore.SaveChangesAsync(token);
        }

        public async Task<PagedList<FileDto<Guid>>> QueryPagedList(LimitQueryInfo req, CancellationToken cancellationToken = default)
        {
            var allOwners = await logicRoleProviders.GetAllRoles(cloudFileOptions.LogicRoleProviders);
            return await entityStore.Current.FilterDeleted().Where(p => allOwners.Contains(p.Owner))
                 .To<FileDto<Guid>>().QueryPageAsync(req, cancellationToken);
        }

        public async Task Rename(RenameFileDto<Guid> renameFileDto, CancellationToken cancellationToken = default)
        {
            var file = await entityStore.Current.FindOrThrowAsync(renameFileDto.Id, cancellationToken);
            file.Name = renameFileDto.Name;
            await entityStore.SaveChangesAsync(cancellationToken);
        }
    }
}

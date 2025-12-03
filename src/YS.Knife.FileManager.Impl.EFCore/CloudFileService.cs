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
    [Mapper(typeof(CreateFolderDto<Guid>), typeof(FileEntity<Guid>), MapperType = MapperType.Convert)]

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
            var mainLogicRole = (await logicRoleProviders.GetAllRoles(cloudFileOptions.MainLogicRoleProvider)).Last();
            var parents = await entityStore.Current.Where(p => p.Owner == mainLogicRole).FilterDeleted().FindDictionaryOrThrowAsync(dtos.Where(p => p.ParentId.HasValue).Select(p => p.ParentId!.Value).Distinct().ToArray(), token);
            var entities = dtos.Select(p => p.To<FileEntity<Guid>>(t =>
            {
                if (t.ParentId.HasValue)
                {
                    if (parents.TryGetValue(t.ParentId.Value, out var parent))
                    {
                        if (!parent.IsFolder)
                        {
                            throw ParentIsNotFolder();
                        }
                    }
                }
                t.IsFolder = false;
                t.Owner = mainLogicRole;
            })).ToArray();

            entityStore.AddRange(entities);
            await entityStore.SaveChangesAsync(token);
            return entities.Select(p => p.Id).ToArray();
        }
        public async Task<Guid[]> CreateFolder(CreateFolderDto<Guid>[] dtos, CancellationToken token = default)
        {
            var mainLogicRole = (await logicRoleProviders.GetAllRoles(cloudFileOptions.MainLogicRoleProvider)).Last();

            var parents = await entityStore.Current.Where(p => p.Owner == mainLogicRole).FilterDeleted().FindDictionaryOrThrowAsync(dtos.Where(p => p.ParentId.HasValue).Select(p => p.ParentId!.Value).Distinct().ToArray(), token);
            var entities = dtos.Select(p => p.To<FileEntity<Guid>>(t =>
            {
                if (t.ParentId.HasValue)
                {
                    if (parents.TryGetValue(t.ParentId.Value, out var parent))
                    {
                        if (!parent.IsFolder)
                        {
                            throw ParentIsNotFolder();
                        }
                    }
                }
                t.IsFolder = false;
                t.Owner = mainLogicRole;
            })).ToArray();
            entityStore.AddRange(entities);
            await entityStore.SaveChangesAsync(token);
            return entities.Select(p => p.Id).ToArray();
        }


        public async Task Delete(Guid[] keys, CancellationToken token = default)
        {
            var mainLogicRole = (await logicRoleProviders.GetAllRoles(cloudFileOptions.MainLogicRoleProvider)).Last();
            var files = await entityStore.Current.Where(p => p.Owner == mainLogicRole).FindArrayOrThrowAsync(keys, token);
            Array.ForEach(files, entityStore.Delete);
            await entityStore.SaveChangesAsync(token);
        }

        public async Task<PagedList<FileDto<Guid>>> QueryPagedList(LimitQueryInfo req, CancellationToken cancellationToken = default)
        {
            var mainLogicRole = (await logicRoleProviders.GetAllRoles(cloudFileOptions.MainLogicRoleProvider)).Last();
            //var allOwners = await logicRoleProviders.GetAllRoles(cloudFileOptions.MainLogicProvider);
            return await entityStore.Current.FilterDeleted().Where(p => mainLogicRole == p.Owner)
                 .To<FileDto<Guid>>().QueryPageAsync(req, cancellationToken);
        }

        public async Task Rename(RenameFileDto<Guid> renameFileDto, CancellationToken cancellationToken = default)
        {
            var mainLogicRole = (await logicRoleProviders.GetAllRoles(cloudFileOptions.MainLogicRoleProvider)).Last();
            var file = await entityStore.Current.Where(p => p.Owner == mainLogicRole).FindOrThrowAsync(renameFileDto.Id, cancellationToken);
            file.Name = renameFileDto.Name;
            await entityStore.SaveChangesAsync(cancellationToken);
        }


    }
}

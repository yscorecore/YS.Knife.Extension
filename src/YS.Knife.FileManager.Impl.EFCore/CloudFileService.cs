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


        [CodeException("002", "文件\"{file}\"已经存在")]
        private partial Exception FileAlreadyExists(string file);

        [CodeException("003", "目录\"{file}\"已经存在")]
        private partial Exception FolderAlreadyExists(string file);

        [CodeException("004", "存在重复的文件名")]
        private partial Exception HasDuplicateFile();

        [CodeException("005", "存在重复的目录名")]
        private partial Exception HasDuplicateFolder();

        public async Task<Guid[]> Create(CreateFileDto<Guid>[] dtos, CancellationToken token = default)
        {
            var mainLogicRole = (await logicRoleProviders.GetAllRoles(cloudFileOptions.MainLogicRoleProvider)).Last();
            var baseQuery = entityStore.Current.Where(p => p.Owner == mainLogicRole).FilterDeleted();
            var parents = await baseQuery.FindDictionaryOrThrowAsync(dtos.Where(p => p.ParentId.HasValue).Select(p => p.ParentId!.Value).Distinct().ToArray(), token);
            if (dtos.HasDuplicate(p => (p.ParentId, p.Name)))
            {
                throw HasDuplicateFile();
            }
            var duplicateName = await baseQuery.WhereItemsOr(dtos, (p, v) => p.ParentId == v.ParentId && p.Name == v.Name)
                .FirstOrDefaultAsync(token);
            if (duplicateName != null)
            {
                throw FileAlreadyExists(duplicateName.Name);
            }
            var entities = dtos.Select(p => p.To<FileEntity<Guid>>(t =>
            {
                if (t.ParentId.HasValue && parents.TryGetValue(t.ParentId.Value, out var parent) && !parent.IsFolder)
                {
                    throw ParentIsNotFolder();
                }
                t.Extension = Path.GetExtension(t.Name);
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
            var baseQuery = entityStore.Current.Where(p => p.Owner == mainLogicRole).FilterDeleted();
            var parents = await baseQuery.FindDictionaryOrThrowAsync(dtos.Where(p => p.ParentId.HasValue).Select(p => p.ParentId!.Value).Distinct().ToArray(), token);
            if (dtos.HasDuplicate(p => (p.ParentId, p.Name)))
            {
                throw HasDuplicateFolder();
            }

            var duplicateName = await baseQuery.WhereItemsOr(dtos, (p, v) => p.ParentId == v.ParentId && p.Name == v.Name)
                .FirstOrDefaultAsync(token);
            if (duplicateName != null)
            {
                throw FolderAlreadyExists(duplicateName.Name);
            }
            var entities = dtos.Select(p => p.To<FileEntity<Guid>>(t =>
            {
                if (t.ParentId.HasValue && parents.TryGetValue(t.ParentId.Value, out var parent) && !parent.IsFolder)
                {
                    throw ParentIsNotFolder();
                }
                t.IsFolder = true;
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
            Array.ForEach(files, p => p.IsDeleted = true);
            await entityStore.SaveChangesAsync(token);
        }

        public async Task<PagedList<FileDto<Guid>>> QueryPagedList(LimitQueryInfo req, CancellationToken cancellationToken = default)
        {
            var mainLogicRole = (await logicRoleProviders.GetAllRoles(cloudFileOptions.MainLogicRoleProvider)).Last();
            return await entityStore.Current.FilterDeleted().Where(p => mainLogicRole == p.Owner)
                 .To<FileDto<Guid>>()
                 .OrderByDescending(p => p.IsFolder)
                 .ThenBy(p => p.Name)
                 .QueryPageAsync(req, cancellationToken);
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

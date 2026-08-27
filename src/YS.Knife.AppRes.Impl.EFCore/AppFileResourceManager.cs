using Microsoft.EntityFrameworkCore;
using YS.Knife.AppRes.Entity.EFCore;
using YS.Knife.EFCore.Services;
using YS.Knife.Entity;
using YS.Knife.Query;
using static YS.Knife.AppRes.IAppFileResourceManager;

namespace YS.Knife.AppRes.Impl.EFCore
{
    [Service(typeof(IAppFileResourceManager))]
    [Service(typeof(IAppFileResourceService))]
    [AutoConstructor]
    [Mixin(typeof(QueryApi<AppFileResourceEntity, AppFileResourceInfo>))]
    [Mixin(typeof(CreateApi<AppFileResourceEntity, AddAppFileResourceDto, Guid>))]
    [Mixin(typeof(UpdateApi<AppFileResourceEntity, EditAppFileResourceDto, Guid>))]
    [Mixin(typeof(DeleteApi<AppFileResourceEntity, Guid>))]
    [Mapper(typeof(AppFileResourceEntity), typeof(AppFileResourceInfo), MapperType = MapperType.Query, CheckType = CheckType.TargetMembersFullFilled)]
    [Mapper(typeof(AppFileResourceEntity), typeof(IAppFileResourceService.AppGroupFileResourceInfo), MapperType = MapperType.Query, CheckType = CheckType.TargetMembersFullFilled)]

    [Mapper(typeof(AddAppFileResourceDto), typeof(AppFileResourceEntity), MapperType = MapperType.Convert, CheckType = CheckType.SourceMembersFullUsed, CustomMappings = new string[] { "Properties = $.Properties" })]
    [Mapper(typeof(EditAppFileResourceDto), typeof(AppFileResourceEntity), MapperType = MapperType.Update, CheckType = CheckType.SourceMembersFullUsed, CustomMappings = new string[] { "Properties = $.Properties" })]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("FlyTiger.Mapper", "FT50000:The mapper is not being used", Justification = "<挂起>")]
    public partial class AppFileResourceManager : IAppFileResourceManager, IAppFileResourceService
    {
        private readonly IEntityStore<AppFileResourceEntity> entityStore;

        public async Task<string> GetUrl(string key, CancellationToken cancellationToken)
        {
            AppFileResourceEntity entity;
            if (Guid.TryParse(key, out var id))
            {
                entity = await entityStore.Current.Where(p => p.Id == id).FindOrThrowAsync(cancellationToken);
            }
            else if (AppTextResourceManager.KeyRegex.IsMatch(key))
            {
                var match = AppTextResourceManager.KeyRegex.Match(key);
                var code = match.Groups["c"].Value;
                var group = match.Groups["g"].Value;
                entity = await entityStore.Current.Where(p => p.Group == group && p.Code == code).FindOrThrowAsync(cancellationToken);
            }
            else
            {
                throw new InvalidOperationException($"Invalid key format: '{key}', expected guid or code@group.");
            }
            return entity.FileUrl;
        }

        public Task<PagedList<IAppFileResourceService.AppGroupFileResourceInfo>> Query(string group, LimitQueryInfo req, CancellationToken cancellationToken = default)
        {
            return entityStore.Current.Where(p => p.Group == group).OrderBy(p => p.Order).To<IAppFileResourceService.AppGroupFileResourceInfo>().QueryPageAsync(req, cancellationToken);
        }
    }

}

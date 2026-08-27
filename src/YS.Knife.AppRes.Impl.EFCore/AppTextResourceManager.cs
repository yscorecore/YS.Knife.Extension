using System.Net.Mime;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using YS.Knife.AppRes.Entity.EFCore;
using YS.Knife.EFCore.Services;
using YS.Knife.Entity;
using YS.Knife.Query;
using static YS.Knife.AppRes.IAppTextResourceManager;
using static YS.Knife.AppRes.IAppTextResourceService;

namespace YS.Knife.AppRes.Impl.EFCore
{
    [Service(typeof(IAppTextResourceManager))]
    [Service(typeof(IAppTextResourceService))]
    [AutoConstructor]
    [Mixin(typeof(QueryApi<AppTextResourceEntity, AppTextResourceInfo>))]
    [Mixin(typeof(CreateApi<AppTextResourceEntity, AddAppTextResourceDto, Guid>))]
    [Mixin(typeof(UpdateApi<AppTextResourceEntity, EditAppTextResourceDto, Guid>))]
    [Mixin(typeof(DeleteApi<AppTextResourceEntity, Guid>))]
    [Mapper(typeof(AppTextResourceEntity), typeof(AppTextResourceInfo), MapperType = MapperType.Query, CheckType = CheckType.TargetMembersFullFilled)]
    [Mapper(typeof(AppTextResourceEntity), typeof(AppGroupTextResourceInfo), MapperType = MapperType.Query, CheckType = CheckType.TargetMembersFullFilled)]
    [Mapper(typeof(AddAppTextResourceDto), typeof(AppTextResourceEntity), MapperType = MapperType.Convert, CheckType = CheckType.SourceMembersFullUsed)]
    [Mapper(typeof(EditAppTextResourceDto), typeof(AppTextResourceEntity), MapperType = MapperType.Update, CheckType = CheckType.SourceMembersFullUsed)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("FlyTiger.Mapper", "FT50000:The mapper is not being used", Justification = "<挂起>")]
    public partial class AppTextResourceManager : IAppTextResourceManager, IAppTextResourceService
    {
        private readonly IEntityStore<AppTextResourceEntity> entityStore;
        private static readonly Regex keyRegex = new Regex("^(?<c>.+)@(?<g>.+)$");

        public async Task<StreamBody> GetContent(string key, CancellationToken cancellationToken)
        {
            AppTextResourceEntity entity;
            if (Guid.TryParse(key, out var id))
            {
                entity = await entityStore.Current.Where(p => p.Id == id).FindOrThrowAsync(cancellationToken);
            }
            else if (keyRegex.IsMatch(key))
            {
                var match = keyRegex.Match(key);
                var code = match.Groups["c"].Value;
                var group = match.Groups["g"].Value;
                entity = await entityStore.Current.Where(p => p.Group == group && p.Code == code).FindOrThrowAsync(cancellationToken);
            }
            else
            {
                throw new InvalidOperationException($"Invalid key format: '{key}', expected guid or code@group.");
            }
            var bytes = Encoding.UTF8.GetBytes(entity.Content ?? string.Empty);
            return StreamBody.FromBytes(bytes, MediaTypeNames.Text.Plain, $"{entity.Name}.txt");
        }

        public Task<PagedList<AppGroupTextResourceInfo>> Query(string group, LimitQueryInfo req, CancellationToken cancellationToken = default)
        {
            return entityStore.Current.Where(p => p.Group == group).OrderBy(p => p.Order).To<AppGroupTextResourceInfo>().QueryPageAsync(req, cancellationToken);

        }
    }
}

using System.Net.Mime;
using System.Text;
using Microsoft.EntityFrameworkCore;
using YS.Knife.AppRes.Entity.EFCore;
using YS.Knife.EFCore.Services;
using YS.Knife.Entity;
using YS.Knife.Query;
using static YS.Knife.AppRes.IAppTextResourceManager;

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
    [Mapper(typeof(AddAppTextResourceDto), typeof(AppTextResourceEntity), MapperType = MapperType.Convert, CheckType = CheckType.SourceMembersFullUsed)]
    [Mapper(typeof(EditAppTextResourceDto), typeof(AppTextResourceEntity), MapperType = MapperType.Update, CheckType = CheckType.SourceMembersFullUsed)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("FlyTiger.Mapper", "FT50000:The mapper is not being used", Justification = "<挂起>")]
    public partial class AppTextResourceManager : IAppTextResourceManager, IAppTextResourceService
    {
        private readonly IEntityStore<AppTextResourceEntity> entityStore;

        public async Task<StreamBody> GetContent(Guid id, CancellationToken cancellationToken)
        {
            var entity = await entityStore.Current.Where(p => p.Id == id).FindOrThrowAsync(cancellationToken);
            var bytes = Encoding.UTF8.GetBytes(entity.Content ?? string.Empty);
            return StreamBody.FromBytes(bytes, MediaTypeNames.Text.Plain, $"{entity.Name}.txt");
        }

        public Task<PagedList<AppTextResourceInfo>> QueryByGroup(string group, LimitQueryInfo req, CancellationToken cancellationToken = default)
        {
            var newQuery = req with { Filter = CombinFilter(Builder.CreateFilter<AppTextResourceInfo>(p => p.Group.StartsWith(group)), req.Filter) };
            return this.QueryPagedList(newQuery, cancellationToken);
        }


        private static string CombinFilter(string filter1, string filter2)
        {
            if (string.IsNullOrEmpty(filter1))
            {
                return filter2;
            }
            if (string.IsNullOrEmpty(filter2))
            {
                return filter1;
            }
            return $"({filter1}) and ({filter2})";
        }

    }
}

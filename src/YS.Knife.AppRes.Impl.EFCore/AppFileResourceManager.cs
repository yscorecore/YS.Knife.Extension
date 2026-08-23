using YS.Knife.AppRes.Entity.EFCore;
using YS.Knife.EFCore.Services;
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
    [Mapper(typeof(AddAppFileResourceDto), typeof(AppFileResourceEntity), MapperType = MapperType.Convert, CheckType = CheckType.SourceMembersFullUsed)]
    [Mapper(typeof(EditAppFileResourceDto), typeof(AppFileResourceEntity), MapperType = MapperType.Update, CheckType = CheckType.SourceMembersFullUsed)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("FlyTiger.Mapper", "FT50000:The mapper is not being used", Justification = "<挂起>")]
    public partial class AppFileResourceManager : IAppFileResourceManager, IAppFileResourceService
    {
        public Task<PagedList<AppFileResourceInfo>> QueryByGroup(string group, LimitQueryInfo req, CancellationToken cancellationToken = default)
        {
            var newQuery = req with { Filter = CombinFilter(Builder.CreateFilter<AppFileResourceInfo>(p => p.Group.StartsWith(group)), req.Filter) };
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

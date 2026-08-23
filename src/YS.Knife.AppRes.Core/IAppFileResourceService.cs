using YS.Knife.Query;
using YS.Knife.Service;
using static YS.Knife.AppRes.IAppFileResourceManager;

namespace YS.Knife.AppRes
{
    public interface IAppFileResourceService
    {
        Task<PagedList<AppFileResourceInfo>> QueryByGroup(string group, LimitQueryInfo req, CancellationToken cancellationToken = default);
     
    }
}

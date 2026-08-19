using YS.Knife.Function.Core;
using YS.Knife.Query;

namespace YS.Knife.Function
{
    /// <summary>
    /// 提供管理功能权限的服务接口
    /// </summary>
    public interface IFunctionManagerService
    {
        Task<FunctionTreeInfo> GetFunctionTree(string appId);

        Task<PagedList<FunctionInfo>> GetApps(LimitQueryInfo req, CancellationToken cancellationToken = default);

        Task SaveFunctions(string appId, List<FunctionInfo> allFunctions, CancellationToken cancellationToken);

        Task DeleteApp(string appId, CancellationToken cancellationToken);

        Task<List<FunctionInfo>> LoadFromFile(StreamBody file, CancellationToken cancellationToken);
    }
}

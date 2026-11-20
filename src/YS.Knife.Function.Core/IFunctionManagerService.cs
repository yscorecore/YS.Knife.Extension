using YS.Knife.Function;
using YS.Knife.Query;

namespace YS.Knife.Function
{
    /// <summary>
    /// 提供管理功能权限的服务接口
    /// </summary>
    public interface IFunctionManagerService
    {
        Task<FunctionTreeInfo> GetFunctionTree(string appId, CancellationToken cancellationToken = default);

        Task<PagedList<FunctionInfo>> GetApps(LimitQueryInfo req, CancellationToken cancellationToken = default);

        Task SaveFunctions(string appId, List<FunctionInfo> allFunctions, CancellationToken cancellationToken = default);

        Task DeleteApp(string appId, CancellationToken cancellationToken = default);

        Task<List<FunctionInfo>> LoadFromFile(StreamBody file, CancellationToken cancellationToken);
        public async Task ImportFromFile(StreamBody file, CancellationToken cancellationToken)
        {
            var functions = await LoadFromFile(file, cancellationToken);
            await SaveFunctions(functions.First().Code, functions, cancellationToken);
        }
        Task RefreshApiFunctions(CancellationToken cancellationToken = default);
    }
}

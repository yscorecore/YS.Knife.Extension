using YS.Knife.Function.Core;

namespace YS.Knife.Function
{
    /// <summary>
    /// 提供管理功能权限的服务接口
    /// </summary>
    public interface IFunctionManagerService
    {
        Task<FunctionTreeInfo> GetFunctionTree(string appId);

        Task<List<FunctionInfo>> GetApps();

        Task SaveFunctions(string appId, List<FunctionInfo> allFunctions, CancellationToken cancellationToken);

        Task DeleteApp(string appId, CancellationToken cancellationToken);

        Task<List<FunctionInfo>> LoadFromFile(StreamBody file, CancellationToken cancellationToken);
    }
}

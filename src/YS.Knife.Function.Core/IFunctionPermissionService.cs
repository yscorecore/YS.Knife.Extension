namespace YS.Knife.Function.Core
{
    public interface IFunctionPermissionService
    {
        Task<bool> HasPermission(string appId, string funcitonCode);

        Task<FunctionTreeInfo> GetPermissionTree(string appId, string? functionCode);
    }
}

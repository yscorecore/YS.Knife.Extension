namespace YS.Knife.Function
{
    public interface IFunctionMockService
    {
        Task<FunctionTreeInfo> MockPermissionTree(string appId, string[] logicRoles);
    }
}

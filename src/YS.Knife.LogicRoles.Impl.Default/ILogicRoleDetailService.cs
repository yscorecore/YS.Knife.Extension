
using static YS.Knife.LogicRoles.ILogicRoleManagerService;

namespace YS.Knife.LogicRoles.Impl.Default
{
    public interface ILogicRoleDetailService
    {
        string Name { get; }
        string Description { get; }
        Task<LogicRoleInfo[]> GetLogicRoles();
    }
}

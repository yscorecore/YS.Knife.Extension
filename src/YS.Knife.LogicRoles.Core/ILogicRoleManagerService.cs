using static YS.Knife.LogicRoles.ILogicRoleManagerService;

namespace YS.Knife.LogicRoles
{
    public interface ILogicRoleManagerService
    {
        public record LogicRoleProviderInfo
        {
            public string Name { get; set; }
            public string Description { get; set; }
        }
        public record LogicRoleInfo
        {
            public string Code { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public List<LogicRoleInfo> Children { get; set; }
        }
        Task<List<LogicRoleProviderInfo>> GetProviders(string[] names);
        Task<LogicRoleInfo[]> GetLogicRoles(string name);
    }
    public interface ILogicRoleDetailService
    {
        string Name { get; }
        string Description { get; }
        Task<LogicRoleInfo[]> GetLogicRoles();
    }
}

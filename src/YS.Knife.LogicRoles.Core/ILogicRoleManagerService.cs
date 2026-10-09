using static YS.Knife.LogicRoles.ILogicRoleManagerService;

namespace YS.Knife.LogicRoles
{
    public interface ILogicRoleManagerService
    {
        public record LogicRoleProviderInfo
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public bool IsTreeNode { get; set; }
            public bool LeafNodeOnly { get; set; }
            public bool MultipleSelection { get; set; }
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
        bool IsTreeNode { get; }
        bool LeafNodeOnly { get; }
        bool MultipleSelection { get; }
        Task<LogicRoleInfo[]> GetLogicRoles();
    }
}

using static YS.Knife.LogicRoles.ILogicRoleManagerService;

namespace YS.Knife.LogicRoles
{
    public interface ILogicRoleManagerService
    {
        public record LogicRoleProviderInfo
        {
            public string Name { get; set; }
            public string Description { get; set; }
            /// <summary>
            /// 是否是树状结构
            /// </summary>
            public bool IsTreeNode { get; set; }
            /// <summary>
            /// 是否允许多选
            /// </summary>
            public bool MultipleSelection { get; set; }
            /// <summary>
            /// 只能选择叶子节点
            /// </summary>
            public bool LeafNodeOnly { get; set; }
            /// <summary>
            /// 是否选择整个树
            /// </summary>
            public bool TreeSelection { get; set; }
        }
        public record LogicRoleInfo
        {
            public string ProviderName { get; set; }
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
        bool TreeSelection { get; }
        Task<LogicRoleInfo[]> GetLogicRoles();
    }
}

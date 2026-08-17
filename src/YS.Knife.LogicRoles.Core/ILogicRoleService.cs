namespace YS.Knife.LogicRoles
{
    public interface ILogicRoleService
    {
        Task<string[]> GetAllRoleCodes();
    }

    public static class LogicRoleExtensions
    {
        public static string[] FilterByProviders(this string[] roleCodes, string[] providers)
        {
            return (providers ?? Array.Empty<string>()).SelectMany(p => roleCodes.Where(t => t.StartsWith(p + "::"))).ToArray();
        }
    }
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
}

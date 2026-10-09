
using static YS.Knife.LogicRoles.ILogicRoleManagerService;

namespace YS.Knife.LogicRoles.Impl.Default
{
    [AutoConstructor]
    [Service]
    public partial class LogicRoleManagerService : ILogicRoleManagerService
    {
        private IEnumerable<ILogicRoleDetailService> services;
        public async Task<LogicRoleInfo[]> GetLogicRoles(string name)
        {
            var provider = services.Single(p => p.Name == name);
            var res = await provider.GetLogicRoles();
            return res.Select(p => CopyRoleInfo(p, provider.Name)).ToArray();
        }
        private LogicRoleInfo CopyRoleInfo(LogicRoleInfo info, string providerName)
        {
            var children = info.Children != null ?
                info.Children.Select(t => CopyRoleInfo(t, providerName)).ToList() : null;
            return new LogicRoleInfo
            {
                Code = string.IsNullOrEmpty(info.ProviderName) ? $"{providerName}::{info.Code}" : $"{info.ProviderName}::{info.Code}",
                Description = info.Description,
                Name = info.Name,
                Children = children
            };
        }

        public Task<List<LogicRoleProviderInfo>> GetProviders(string[] names)
        {
            var providers = names.Select((s, i) =>
            {
                var detail = services.Where(p => p.Name == s).Single();
                return new LogicRoleProviderInfo
                {
                    Name = s,
                    Description = detail.Description,
                    IsTreeNode = detail.IsTreeNode,
                    LeafNodeOnly = detail.LeafNodeOnly,
                    MultipleSelection = detail.MultipleSelection,
                    TreeSelection = detail.TreeSelection,
                };
            })
            .ToList();
            return Task.FromResult(providers);
        }


    }
}

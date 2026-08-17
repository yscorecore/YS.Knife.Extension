
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
                Code = $"{providerName}::{info.Code}",
                Description = info.Description,
                Name = info.Name,
                Children = children
            };
        }

        public Task<List<LogicRoleProviderInfo>> GetProviders(string[] names)
        {
            var providers = names.Select((s, i) => new LogicRoleProviderInfo
            {
                Name = s,
                Description = services.Where(p => p.Name == s).Select(p => p.Description).FirstOrDefault() ?? s
            })
            .ToList();
            return Task.FromResult(providers);
        }


    }
}

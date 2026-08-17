namespace YS.Knife.LogicRoles
{
    public interface ILogicRoleProvider
    {
        string Name { get; }
        Task<string[]> GetCurrentRoleCodes();
    }

    public static class LogicRoleProviderExtensions
    {
        public static async Task<IList<string>> GetAllRoles(this IEnumerable<ILogicRoleProvider> allProviders, string[] activeProviderNames)
        {
            var logicRoleMap = allProviders.ToDictionary(p => p.Name);
            var res = new List<string>();
            foreach (var name in activeProviderNames)
            {
                if (logicRoleMap.TryGetValue(name, out var provider))
                {
                    var cleanRoleCodes = await provider.GetCurrentRoleCodes();
                    res.AddRange(cleanRoleCodes.Select(p => $"{provider.Name}::{p}"));
                }
                else
                {
                    throw new Exception($"Can not find logic role provider '{name}'.");
                }
            }
            return res;
        }
        public static Task<IList<string>> GetAllRoles(this IEnumerable<ILogicRoleProvider> allProviders, string activeProviderName)
        {
            return allProviders.GetAllRoles(new string[] { activeProviderName });
        }
        public static Task<string> GetMainRole(this IEnumerable<ILogicRoleProvider> allProviders, string activeProviderName)
        {
            return GetAllRoles(allProviders, activeProviderName).ContinueWith(p => p.Result.Last());
        }
    }

}

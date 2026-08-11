
namespace YS.Knife.LogicRoles.Impl.Default
{
    [Service]
    [AutoConstructor]
    public partial class DefaultLogicRoleService : ILogicRoleService
    {
        private readonly IEnumerable<ILogicRoleProvider> _providers;

        public async Task<string[]> GetAllRoleCodes()
        {
            List<string> allRoles = new List<string>();
            foreach (var provider in _providers)
            {
                allRoles.AddRange(await provider.GetCurrentRoleCodes());
            }
            return allRoles.ToArray();
        }
    }
}

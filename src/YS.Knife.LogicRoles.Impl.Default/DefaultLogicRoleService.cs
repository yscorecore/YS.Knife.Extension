namespace YS.Knife.LogicRoles.Impl.Default
{
    [Service]
    [AutoConstructor]
    public partial class DefaultLogicRoleService : ILogicRoleService
    {
        private readonly IEnumerable<ILogicRoleProvider> _providers;

        public async Task<string[]> GetAllRoleCodes()
        {
            return (await _providers.GetAllRoles()).ToArray();
        }
    }
}

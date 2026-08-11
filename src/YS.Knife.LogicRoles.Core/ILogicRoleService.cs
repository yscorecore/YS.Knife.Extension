namespace YS.Knife.LogicRoles
{
    public interface ILogicRoleService
    {
        Task<string[]> GetALlRoleCodes();
    }

    public static class LogicRoleExtensions
    {
        public static string[] FilterByProviders(this string[] roleCodes, string[] providers)
        {
            return (providers ?? Array.Empty<string>()).SelectMany(p => roleCodes.Where(t => t.StartsWith(p + "::"))).ToArray();
        }
    }
}


namespace YS.Knife.LogicRoles.Impl.HttpContextUser
{
    [Service]
    [AutoConstructor]
    public partial class HttpContextLogicRoleService : ILogicRoleService
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        public Task<string[]> GetAllRoleCodes()
        {
            return Task.FromResult((httpContextAccessor.HttpContext?.User.Claims.Where(p => p.Type == ClaimTypes.LogicRole)
                  .Select(p => p.Value).ToArray() ?? Array.Empty<string>()));
        }
    }
}


using YS.Knife;
using YS.Knife.KeyValue;

namespace KeyValueDemo
{
    [Service(typeof(UserSetting))]
    public class UserSetting : KeyValueGroup
    {
        public override Task<string> GetKeyPrefix()
        {
            return Task.FromResult($"userstting::{Thread.CurrentPrincipal?.Identity?.Name}");
        }
    }
    [Service(typeof(OrgSetting))]
    public class OrgSetting : KeyValueGroup
    {
        public override Task<string> GetKeyPrefix()
        {
            return Task.FromResult($"orgstting::{Thread.CurrentPrincipal?.Identity?.Name}");
        }
    }
}

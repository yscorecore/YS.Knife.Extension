
using YS.Knife;
using YS.Knife.KeyValue;

namespace KeyValueDemo
{
    [Service(typeof(UserSetting), ServiceLifetime.Singleton)]
    public class UserSetting : KeyValueGroup
    {
        public override string BuildUniqueKey(string key)
        {
            return $"userstting::{Thread.CurrentPrincipal?.Identity?.Name}";
        }
    }
    [Service(typeof(OrgSetting))]
    public class OrgSetting : KeyValueGroup
    {
        public override string BuildUniqueKey(string key)
        {
            return $"orgstting::{Thread.CurrentPrincipal?.Identity?.Name}";
        }
    }
}

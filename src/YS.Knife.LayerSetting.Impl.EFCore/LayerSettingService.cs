
using Microsoft.EntityFrameworkCore;
using YS.Knife.Entity;
using YS.Knife.LayerSetting.Entity.EFCore;
using YS.Knife.LogicRoles;

namespace YS.Knife.Function.Impl.EFCore
{
    [AutoConstructor]
    [Service]
    public partial class LayerSettingService : ILayerSettingService
    {
        private readonly ILayerService layerService;
        private readonly IEntityStore<SettingEntity> settings;
        private readonly ILogicRoleService logicRoleService;


        private async Task<string[]> GetGroupProviderNames(string group)
        {
            return (await settings.Current.Where(p => p.Group == group)
               .Select(p => p.RoleProviders)
               .SingleOrDefaultAsync()) ?? Array.Empty<string>();
        }

        public async Task<Dictionary<string, object>> GetLayerSetting(string group)
        {
            var providerNames = await GetGroupProviderNames(group);
            var allRoleCodes = await logicRoleService.GetAllRoleCodes();
            return await layerService.GetLayerSettingDictionaryByRoleCodes(group, allRoleCodes.FilterByProviders(providerNames));
        }



        public async Task<T> GetLayerSettingObject<T>(string group) where T : class, new()
        {
            var providerNames = await GetGroupProviderNames(group);
            var allRoleCodes = await logicRoleService.GetAllRoleCodes();
            return await layerService.GetLayerSettingObjectByRoleCodes<T>(group, allRoleCodes.FilterByProviders(providerNames));
        }


    }
}

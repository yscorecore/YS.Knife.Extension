using Microsoft.EntityFrameworkCore;
using YS.Knife.Entity;
using YS.Knife.Function;
using YS.Knife.LayerSetting.Core;
using YS.Knife.LayerSetting.Entity.EFCore;
using YS.Knife.Query;


namespace YS.Knife.LayerSetting.Impl.EFCore
{
    [Service]
    [AutoConstructor]
    [Mapper(typeof(SettingEntity), typeof(SettingInfo), MapperType = MapperType.Query)]
    [Mapper(typeof(SettingInfo), typeof(SettingEntity), MapperType = MapperType.BatchUpdate)]
    public partial class LayerSettingManagerService : ILayerSettingManagerService
    {
        private readonly IEntityStore<SettingEntity> settingStore;
        private readonly IEntityStore<SettingPropertyEntity> settingPropertyStore;
        public Task<SettingInfo> LoadFromFile(StreamBody file, CancellationToken cancellationToken)
        {
            return SettingFile.Instance.LoadFromFile(file, cancellationToken);
        }

        public Task<PagedList<SettingInfo>> QuerySettings(LimitQueryInfo req, CancellationToken cancellationToken = default)
        {
            return settingStore.Current.To<SettingInfo>().QueryPageAsync(req, cancellationToken);
        }

        public async Task RemoveSetting(string group, CancellationToken cancellationToken = default)
        {
            var setting = settingStore.Current.Include(p => p.Properties).Single(p => p.Group == group);
            settingPropertyStore.DeleteRange(setting.Properties);
            settingStore.Delete(setting);
            await settingStore.SaveChangesAsync(cancellationToken);
        }

        public async Task SaveSetting(SettingInfo setting, CancellationToken cancellationToken = default)
        {
            var current = settingStore.Current.Include(p => p.Properties)
                 .FirstOrDefault(p => p.Group == setting.Group);
            if (current == null)
            {
                settingStore.Add(setting.To<SettingEntity>());
            }
            else
            {
                setting.To(current, (t) => { settingPropertyStore.Delete((SettingPropertyEntity)t); });
            }
            await settingStore.SaveChangesAsync();
        }
    }
}

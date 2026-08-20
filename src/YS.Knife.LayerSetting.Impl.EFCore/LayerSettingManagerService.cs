using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using YS.Knife.Entity;
using YS.Knife.Function;
using YS.Knife.LayerSetting.Entity.EFCore;
using YS.Knife.LayerSetting.Files;
using YS.Knife.Query;
using YS.Knife.Service;


namespace YS.Knife.LayerSetting.Impl.EFCore
{
    [Service]
    [AutoConstructor]
    [Mapper(typeof(SettingEntity), typeof(SettingInfo), MapperType = MapperType.Query)]
    [Mapper(typeof(SettingInfo), typeof(SettingEntity), MapperType = MapperType.Convert)]
    [Mapper(typeof(SettingPropertyInfo), typeof(SettingPropertyEntity), MapperType = MapperType.BatchUpdate)]
    public partial class LayerSettingManagerService : ILayerSettingManagerService
    {
        private readonly IEntityStore<SettingEntity> settingStore;
        private readonly IEntityStore<SettingPropertyEntity> settingPropertyStore;
        private readonly LayerSettingOptions options;
        public async Task<SettingInfo> LoadFromFile(StreamBody file, CancellationToken cancellationToken)
        {
            var fileSetting = await FileSettingInfo.LoadFromFile(file, cancellationToken);
            return fileSetting.ToSettingModel();
        }

        public Task<PagedList<SettingInfo>> QuerySettings(LimitQueryInfo req, CancellationToken cancellationToken = default)
        {
            return settingStore.Current.To<SettingInfo>().QueryPageAsync(req, cancellationToken);
        }

        public Task RefreshSettings(CancellationToken cancellationToken = default)
        {
            var settings = new List<SettingInfo>();
            foreach (var ass in (options.Assemblies ?? Array.Empty<string>()))
            {
                settings.AddRange(Assembly.Load(ass).FindLayerSettings().ToList());
            }
            return SaveSettings(settings.ToArray(), saveMode: SaveMode.Merge, cancellationToken);
        }

        public async Task RemoveSetting(string group, CancellationToken cancellationToken = default)
        {
            var setting = settingStore.Current.Include(p => p.Properties).Single(p => p.Group == group);
            settingPropertyStore.DeleteRange(setting.Properties);
            settingStore.Delete(setting);
            await settingStore.SaveChangesAsync(cancellationToken);
        }
        private async Task SaveSettings(IList<SettingInfo> settings, SaveMode saveMode = SaveMode.Merge, CancellationToken cancellationToken = default)
        {
            if (settings.Count == 0)
            {
                return;
            }
            var allGroups = settings.Select(p => p.Group).ToHashSet();
            var currents = await settingStore.Current.Include(p => p.Properties)
                 .Where(p => allGroups.Contains(p.Group))
                 .ToListAsync(cancellationToken);
            var currentDic = currents.ToDictionary(p => p.Group);
            foreach (var s in settings)
            {
                if (currentDic.TryGetValue(s.Group, out var current))
                {
                    s.CopyTo(current);
                    s.Properties.To(current.Properties, (CollectionUpdateMode)(int)saveMode);
                }
                else
                {
                    settingStore.Add(s.To<SettingEntity>());
                }
            }
            await settingStore.SaveChangesAsync(cancellationToken);
        }
        public Task SaveSetting(SettingInfo setting, SaveMode saveMode = SaveMode.Merge, CancellationToken cancellationToken = default)
        {
            return SaveSettings(setting.AsList(), saveMode, cancellationToken);
        }
    }

    [Options]
    public class LayerSettingOptions
    {
        public string[] Assemblies { get; set; } = new string[0];
    }
}

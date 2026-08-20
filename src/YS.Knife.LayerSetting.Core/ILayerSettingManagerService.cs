using System.ComponentModel.DataAnnotations;
using YS.Knife.Query;
using YS.Knife.Service;

namespace YS.Knife.Function
{
    /// <summary>
    /// 提供管理分层配置的服务接口
    /// </summary>
    public interface ILayerSettingManagerService
    {
        Task<PagedList<SettingInfo>> QuerySettings(LimitQueryInfo req, CancellationToken cancellationToken = default);

        Task RemoveSetting(string group, CancellationToken cancellationToken = default);

        Task SaveSetting(SettingInfo setting, SaveMode saveMode = SaveMode.Merge, CancellationToken cancellationToken = default);

        Task<SettingInfo> LoadFromFile(StreamBody file, CancellationToken cancellationToken = default);

        public async Task ImportFromFile(StreamBody file, SaveMode saveMode = SaveMode.Merge, CancellationToken cancellationToken = default)
        {
            var setting = await LoadFromFile(file, cancellationToken);
            await SaveSetting(setting, saveMode, cancellationToken);
        }
        Task RefreshSettings(CancellationToken cancellationToken = default);

    }
    public record SettingInfo
    {
        [Required]
        public string Group { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        [Required]
        public string[] RoleProviders { get; set; }
        public List<SettingPropertyInfo> Properties { get; set; } = new List<SettingPropertyInfo>();
    }
    public record SettingPropertyInfo
    {

        [Required]
        [Key]
        public string Key { get; set; }

        // [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        public bool IsArray { get; set; }

        [Required]
        public string Type { get; set; }

        public string DataSource { get; set; }

        public int Order { get; set; }
    }

}

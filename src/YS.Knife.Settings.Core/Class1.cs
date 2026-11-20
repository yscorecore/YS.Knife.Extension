namespace YS.Knife.Settings.Core
{
    public interface ISettingsService
    {

        Task<string> GetSetting<T>(string key);

        Task SetSetting<T>(string key, T value);
    }


    public interface IBaseSettingInfo
    { 
        Task<string> GetGroupName();
    }
    
    public class UserSetting : IBaseSettingInfo
    {
        public Task<string> GetGroupName()
        {
           return Task.FromResult(string.Empty);
        }
    }

    public class SchoolSetting : IBaseSettingInfo
    {
        public Task<string> GetGroupName()
        {
            return Task.FromResult(string.Empty);
        }
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YS.Knife.Function;

namespace YS.Knife.LayerSetting.Files
{
    public static class SettingInfoExtensions
    {
        public static SettingInfo ToSettingModel(this FileSettingInfo fileSetting)
        {
            return new SettingInfo
            {
                Name = fileSetting.Name,
                Description = fileSetting.Desc,
                Group = fileSetting.Group,
                RoleProviders = fileSetting.RoleProviders,
                Properties = fileSetting.Properties.Select((p, i) => new SettingPropertyInfo
                {
                    Name = p.Value.Name,
                    Description = p.Value.Desc,
                    Key = p.Key,
                    Type = p.Value.Type,
                    IsArray = p.Value.IsArray,
                    Order = i * 100,
                    DataSource = p.Value.DataSource,
                }).ToList(),
            };
        }
        public static FileSettingInfo ToFileSettingModel(this SettingInfo res)
        {
            return new FileSettingInfo
            {
                Name = res.Name,
                Desc = res.Description,
                Group = res.Group,
                RoleProviders = res.RoleProviders,
                Properties = res.Properties.OrderBy(p => p.Order).ToDictionary(p => p.Key, p => new FileSettingPropertyInfo
                {
                    Name = p.Name,
                    Desc = p.Description,
                    Type = p.Type,
                    IsArray = p.IsArray,
                    DataSource = p.DataSource,
                }),
            };
        }
    }
}

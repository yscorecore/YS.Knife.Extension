using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using YS.Knife.Function;
using YS.Knife.Query;

namespace YS.Knife.LayerSetting.Core
{
    [SingletonPattern]
    public partial class SettingFile
    {
        record TempSettingInfo
        {
            public string Group { get; set; }
            public string Name { get; set; }
            public string Desc { get; set; }
            public string[] RoleProviders { get; set; }
            public Dictionary<string, TempSettingPropertyInfo> Properties { get; set; }

        }
        record TempSettingPropertyInfo
        {
            public string Name { get; set; }
            public string Desc { get; set; }
            public bool IsArray { get; set; }
            public string Type { get; set; }
            public string DataSource { get; set; }
        }
        static JsonSerializerOptions JsonSerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            IgnoreReadOnlyProperties = false,
            PropertyNameCaseInsensitive = false,
            ReadCommentHandling = JsonCommentHandling.Skip,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };
        public async Task<SettingInfo> LoadFromFile(StreamBody file, CancellationToken cancellationToken)
        {
            var res = await JsonSerializer.DeserializeAsync<TempSettingInfo>(file.Stream, JsonSerializerOptions);
            return new SettingInfo
            {
                Name = res.Name,
                Description = res.Desc,
                Group = res.Group,
                RoleProviders = res.RoleProviders,
                Properties = res.Properties.Select((p, i) => new SettingPropertyInfo
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


    }
}

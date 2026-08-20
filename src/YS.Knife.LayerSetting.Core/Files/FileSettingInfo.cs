using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace YS.Knife.LayerSetting.Files
{
    public class FileSettingInfo
    {
        internal static readonly JsonSerializerOptions JsonSerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
            IgnoreReadOnlyProperties = false,
            PropertyNameCaseInsensitive = false,
            ReadCommentHandling = JsonCommentHandling.Skip,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
        };

        [JsonPropertyOrder(1)]
        public string Group { get; set; }
        [JsonPropertyOrder(2)]
        public string Name { get; set; }
        [JsonPropertyOrder(3)]
        public string Desc { get; set; }
        [JsonPropertyOrder(4)]
        public string[] RoleProviders { get; set; }
        [JsonPropertyOrder(5)]
        public Dictionary<string, FileSettingPropertyInfo> Properties { get; set; }

        public static async Task<FileSettingInfo> LoadFromFile(StreamBody file, CancellationToken cancellationToken = default)
        {
            return await JsonSerializer.DeserializeAsync<FileSettingInfo>(file.Stream, JsonSerializerOptions, cancellationToken);
        }

        public async Task DumpToFolder(string folder)
        {
            if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            var file = Path.Combine(folder, $"{Group}.json");
            await using var fstream = File.Create(file);
            await JsonSerializer.SerializeAsync(fstream, this, JsonSerializerOptions);
        }
    }
}

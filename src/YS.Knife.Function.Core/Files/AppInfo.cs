using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace YS.Knife.Function.Files
{
    public class AppInfo
    {
        internal static readonly JsonSerializerOptions JsonSerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            IgnoreReadOnlyProperties = false,
            PropertyNameCaseInsensitive = false,
            ReadCommentHandling = JsonCommentHandling.Skip,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
        };

        public string AppId { get; set; } = null!;
        public string AppName { get; set; } = null!;
        public string? AppDesc { get; set; } = null!;
        public Dictionary<string, object> AppConfig { get; set; } = null!;
        public List<ModuleInfo>? Modules { get; set; } = null!;

        public static async Task<AppInfo?> LoadFromFile(StreamBody file, CancellationToken cancellationToken = default)
        {
            return await JsonSerializer.DeserializeAsync<AppInfo>(file.Stream, JsonSerializerOptions, cancellationToken);
        }

        public async Task DumpToFolder(string folder)
        {
            if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            var file = Path.Combine(folder, $"{AppId}.json");
            await using var fstream = File.Create(file);
            await JsonSerializer.SerializeAsync(fstream, this, JsonSerializerOptions);
        }
    }
}

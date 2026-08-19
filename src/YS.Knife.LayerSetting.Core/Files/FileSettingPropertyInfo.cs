using System.Text.Json.Serialization;

namespace YS.Knife.LayerSetting.Files
{
    public class FileSettingPropertyInfo
    {
        [JsonPropertyOrder(1)]
        public string Name { get; set; }
        [JsonPropertyOrder(2)]
        public string Desc { get; set; }
        [JsonPropertyOrder(3)]
        public bool IsArray { get; set; }
        [JsonPropertyOrder(4)]
        public string Type { get; set; }
        [JsonPropertyOrder(5)]
        public string DataSource { get; set; }
    }
}

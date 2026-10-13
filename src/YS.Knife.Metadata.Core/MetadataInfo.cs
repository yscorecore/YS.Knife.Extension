using System.Text.Json.Serialization;

namespace YS.Knife.Metadata
{

    public record MetadataInfo
    {
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public List<MetadataClolumnInfo> Columns { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, string> Tags { get; set; }
    }

}

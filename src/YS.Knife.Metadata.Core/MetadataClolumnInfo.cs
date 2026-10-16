using System.Text.Json.Serialization;

namespace YS.Knife.Metadata
{
    public record MetadataClolumnInfo
    {

        public string PropertyPath { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public bool ShowForDisplay { get; set; }
        public bool Required { get; set; }
        public string DisplayFormat { get; set; }
        public bool IsArray { get; set; }
        public string DataTypeName { get; set; }
        public int DisplayOrder { get; set; }
        public DataSourceInfo DataSource { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DataSourceConfig BindTo { get; set; }
        public QueryFilterInfo QueryFilter { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, object> Tags { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Prompt { get; set; }
    }
    public record DataSourceInfo
    {
        public SourceType Type { get; set; }
        public object Value { get; set; }
        public object Config { get; set; }
        public string Expression { get; set; }
    }

}

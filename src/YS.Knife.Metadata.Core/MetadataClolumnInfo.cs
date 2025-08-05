namespace YS.Knife.Metadata
{
    public record MetadataClolumnInfo
    {

        public string PropertyPath { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public bool ShowForDisplay { get; set; }
        public string DisplayFormat { get; set; }
        public bool IsArray { get; set; }
        public string DataTypeName { get; set; }
        public string DisplayWidth { get; set; } = "auto";//auto,100px,50%
        public int DisplayOrder { get; set; }
        public DataSourceInfo DataSource { get; set; }
        public QueryFilterInfo QueryFilter { get; set; }
    }
    public record DataSourceInfo
    {
        public SourceType Type { get; set; }
        public object Value { get; set; }
    }

}

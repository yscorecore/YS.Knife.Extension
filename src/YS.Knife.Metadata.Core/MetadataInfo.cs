namespace YS.Knife.Metadata
{

    public record MetadataInfo
    {
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public List<MetadataClolumnInfo> Columns { get; set; }
    }

    public record MetadataClolumnInfo
    {

        public string PropertyPath { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public bool ShowForDisplay { get; set; }
        public string DisplayFormat { get; set; }
        public bool IsArray { get; set; }
        public string DataTypeName { get; set; }
        public string EditorSource { get; set; }
    }
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    public class MetadataColumnAttribute : Attribute
    {
        public string PropertyName { get; set; }
    }
}

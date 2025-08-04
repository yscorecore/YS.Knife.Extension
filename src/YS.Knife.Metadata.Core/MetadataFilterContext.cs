namespace YS.Knife.Metadata
{
    public record MetadataFilterContext
    {
        public string Name { get; set; }
        public MetadataInfo MetadataInfo { get; set; }
        
    }
}

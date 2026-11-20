namespace YS.Knife.Metadata
{
    public record MetadataInterceptorContext
    {
        public string MetadataName { get; set; }
        public MetadataInfo MetadataInfo { get; set; }
    }
}

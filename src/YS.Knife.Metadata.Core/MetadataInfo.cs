namespace YS.Knife.Metadata
{

    public record MetadataInfo
    {
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public List<MetadataClolumnInfo> Columns { get; set; }
    }

}

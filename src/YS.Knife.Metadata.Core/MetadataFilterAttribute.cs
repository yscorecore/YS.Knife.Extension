namespace YS.Knife.Metadata
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class MetadataFilterAttribute : Attribute
    {
        public MetadataFilterAttribute(string filterName)
        {
            _ = filterName ?? throw new ArgumentNullException(nameof(filterName));
            FilterName = filterName;
        }

        public string FilterName { get; }
    }
}

namespace YS.Knife.Metadata
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public class MetadataAttribute : Attribute
    {
        public MetadataAttribute()
        {
        }
        public MetadataAttribute(string name)
        {
            this.Name = name;
        }
        public string Name { get; }
    }
}

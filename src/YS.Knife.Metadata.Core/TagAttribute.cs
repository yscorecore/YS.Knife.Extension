namespace YS.Knife.Metadata
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
    public class TagAttribute : Attribute
    {
        public TagAttribute(string key, string value)
        {
            this.Key = key;
            this.Value = value;
        }
        public string Key { get; }
        public string Value { get; }
    }
}

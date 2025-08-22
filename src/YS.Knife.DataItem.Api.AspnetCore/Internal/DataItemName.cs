namespace YS.Knife.DataItem.Api.AspnetCore.Internal
{
    [AttributeUsage(AttributeTargets.Parameter)]
    internal class DataItemNameAttribute : Attribute
    {
        public DataItemNameAttribute(string name)
        {
            Name = name;
        }
        public string Name { get; }
    }
}

namespace YS.Knife.DataItem.Api.AspnetCore
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

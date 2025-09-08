namespace YS.Knife.DataSource.Api.AspnetCore
{
    [AttributeUsage(AttributeTargets.Parameter)]
    internal class DataSourceNameAttribute : Attribute
    {
        public DataSourceNameAttribute(string name)
        {
            Name = name;
        }
        public string Name { get; }
    }
}

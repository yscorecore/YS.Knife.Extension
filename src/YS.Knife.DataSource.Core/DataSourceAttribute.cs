using System.ComponentModel;

namespace YS.Knife.DataItem
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public class DataSourceAttribute : Attribute
    {
        public DataSourceAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
        public object[] Arguments { get; set; } = Array.Empty<object>();
        public bool AutoRegisterMeta { get; set; } = true;
    }
}

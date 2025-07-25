using System.ComponentModel;

namespace YS.Knife.DataSource
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class DataSourceAttribute : Attribute
    {
        public DataSourceAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
        public bool AutoRegisterMeta { get; set; } = true;
    }
}

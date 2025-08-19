namespace YS.Knife.DataItem
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public class DataItemAttribute : Attribute
    {
        public DataItemAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
        public bool AutoRegisterMeta { get; set; } = true;
    }
}

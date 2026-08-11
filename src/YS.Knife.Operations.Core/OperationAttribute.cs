namespace YS.Knife.Operations
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class OperationAttribute : Attribute
    {
        public OperationAttribute(string id, string description)
        {
            Id = id;
            Description = description;
        }

        public string Id { get; }
        public string Description { get; }

    }
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public class ModuleAttribute : Attribute
    {
        public ModuleAttribute(string id, string description)
        {
            Id = id;
            Description = description;
        }

        public string Id { get; }
        public string Description { get; }
    }
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
    public class AppAttribute : Attribute
    {
        public AppAttribute(string id, string description, params string[] roleProviders)
        {
            Id = id;
            Description = description;
        }
        public string Id { get; }
        public string Description { get; }
    }
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public class OperationArgumentAttribute : Attribute
    {
        public OperationArgumentAttribute(string key, string value)
        {
            this.Key = key;
            this.Value = value;
        }
        public string Key { get; set; }
        public string Value { get; set; }
    }
}

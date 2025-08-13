using System.Runtime.CompilerServices;

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
}

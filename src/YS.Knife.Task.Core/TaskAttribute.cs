using System.ComponentModel;
using System.Reflection;

namespace YS.Knife.Task
{
    [System.AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public sealed class TaskAttribute : DescriptionAttribute
    {
        public TaskAttribute(string name, string description, string? version = default) : base(description)
        {
            Name = name;
            Version = version;
        }

        public string Name { get; }
        public string? Version { get; }
        public string? Group { get; }
        public static TaskAttribute GetFromType<T>()
        {
            return GetFromType(typeof(T));
        }
        public static TaskAttribute GetFromType(Type type)
        {
            return type.GetCustomAttribute<TaskAttribute>(true) ?? throw new InvalidOperationException($"Task {type.FullName} does not have TaskAttribute"); ;
        }
    }

}

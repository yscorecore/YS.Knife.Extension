using System.Reflection;

namespace YS.Knife.Task
{
    [System.AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public sealed class TaskAttribute : Attribute
    {
        public TaskAttribute(string name, string? version = default)
        {
            Name = name;
            Version = version;
        }

        public string Name { get; }
        public string? Version { get; }

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

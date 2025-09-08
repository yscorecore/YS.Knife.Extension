using System.Reflection;

namespace YS.Knife.DataItem
{
    public record DataItemEntry
    {
        public string Name { get; set; }
        public Type ServiceType { get; set; }
        public MethodInfo Method { get; set; }
        public Type ReturnType { get; set; }
        public ParameterInfo[] Parameters { get; set; }
        public bool IsValueTask { get; set; }
        public bool HasCancellationToken { get; set; }
        public bool AutoRegisterMeta { get; set; }

    }
}

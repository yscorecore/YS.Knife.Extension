namespace YS.Knife.Operations
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class OperationAttribute : Attribute
    {
        public OperationAttribute(string id, string description, OperationNamingStyle namingStyle = OperationNamingStyle.CamelCase)
        {
            Id = id;
            Description = description;
            NamingStyle = namingStyle;
        }

        public string Id { get; }
        public string Description { get; }

        /// <summary>
        /// <see cref="Id"/> 中占位符替换为声明类型泛型实参名称时使用的命名风格。
        /// </summary>
        public OperationNamingStyle NamingStyle { get; }
    }
}

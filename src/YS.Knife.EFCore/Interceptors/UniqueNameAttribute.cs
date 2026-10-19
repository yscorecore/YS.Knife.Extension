namespace YS.Knife.EFCore
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class UniqueNameAttribute : Attribute
    {
        /// <summary>与当前属性共同构成唯一范围的其他属性名。为空表示全局唯一。</summary>
        public string[] ScopeProperties { get; }

        /// <summary>是否忽略大小写（仅 string 生效）。</summary>
        public bool IgnoreCase { get; set; }

        /// <summary>错误消息模板，{0}=属性名，{1}=值，{2}=作用域描述。</summary>
        public string ErrorMessage { get; set; } =
            "字段 '{0}' 的值 '{1}' 在指定范围内已存在。";

        public UniqueNameAttribute(params string[] scopeProperties)
        {
            ScopeProperties = scopeProperties ?? Array.Empty<string>();
        }
    }
}

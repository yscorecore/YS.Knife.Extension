namespace YS.Knife
{
    /// <summary>
    /// 标注在状态枚举字段上，声明允许从哪些状态流转到该状态；未标注的状态不限制来源。
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class FromStatusAttribute : Attribute
    {
        public FromStatusAttribute(params object[] values)
        {
            this.Values = values;
        }
        public object[] Values { get; set; }

        /// <summary>
        /// 判断是否允许从 current 状态流转到 newValue 状态；目标状态未标注 [FromStatus] 时不限制来源。
        /// </summary>
        public static bool IsValidTransition<TStatus>(TStatus current, TStatus newValue)
             where TStatus : struct, Enum
        {
            var targetField = typeof(TStatus).GetField(newValue.ToString());
            if (targetField == null || targetField.GetCustomAttributes(typeof(FromStatusAttribute), false).FirstOrDefault() is not FromStatusAttribute fromStatusAttr)
            {
                return true;
            }
            if (fromStatusAttr.Values.Any(v => v is not TStatus))
            {
                throw new InvalidOperationException($"[FromStatus] values on field '{typeof(TStatus).Name}.{targetField.Name}' must be of enum type '{typeof(TStatus).Name}'.");
            }
            return fromStatusAttr.Values.Contains(current);
        }
    }
}

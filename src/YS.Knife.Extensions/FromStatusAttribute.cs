namespace YS.Knife
{
    /// <summary>
    /// 标注在状态枚举字段上，声明从该状态允许流转到的目标状态集合；未标注的状态不限制流转。
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class FromStatusAttribute : Attribute
    {
        public FromStatusAttribute(Type enumType, params object[] values)
        {
            if (!enumType.IsEnum)
            {
                throw new ArgumentException("enumType must be an enum type", nameof(enumType));
            }
            Array.ForEach(values, v =>
            {
                if (v == null || v.GetType() != enumType)
                {
                    throw new ArgumentException("values must be of the same type as enumType", nameof(values));
                }
            });
            this.EnumType = enumType;
            this.Values = values;
        }
        public object[] Values { get; set; }
        public Type EnumType { get; set; }

        /// <summary>
        /// 判断从 current 状态是否允许流转到 newValue 状态；未标注 [FromStatus] 的状态不限制流转。
        /// </summary>
        public static bool IsValidTransition<TStatus>(TStatus current, TStatus newValue)
             where TStatus : struct, Enum
        {
            var currentField = typeof(TStatus).GetField(current.ToString());
            if (currentField == null || currentField.GetCustomAttributes(typeof(FromStatusAttribute), false).FirstOrDefault() is not FromStatusAttribute fromStatusAttr)
            {
                return true;
            }
            return fromStatusAttr.Values.Contains(newValue);
        }
    }
}

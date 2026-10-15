namespace YS.Knife
{
    /// <summary>
    /// 标注在状态枚举字段上，声明从该状态允许流转到的目标状态集合；未标注的状态不限制流转。
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

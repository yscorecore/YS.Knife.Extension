namespace YS.Knife.Operations
{
    /// <summary>
    /// 用于控制 <see cref="OperationAttribute"/> 中 id 占位符替换时，类型名称的命名风格。
    /// </summary>
    public enum OperationNamingStyle
    {
        /// <summary>
        /// 原样保留类型名称，例如 User。
        /// </summary>
        Original,

        /// <summary>
        /// 小驼峰命名，首字母小写，例如 user、userProfile。
        /// </summary>
        CamelCase,

        /// <summary>
        /// 大驼峰命名，首字母大写，例如 User、UserProfile。
        /// </summary>
        PascalCase,

        /// <summary>
        /// 全部小写，例如 user、userprofile。
        /// </summary>
        LowerCase,

        /// <summary>
        /// 全部大写，例如 USER、USERPROFILE。
        /// </summary>
        UpperCase,
    }
}

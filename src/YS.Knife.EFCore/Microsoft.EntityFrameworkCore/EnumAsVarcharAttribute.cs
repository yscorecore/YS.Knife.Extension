namespace Microsoft.EntityFrameworkCore
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class EnumAsVarcharAttribute : Attribute
    {
        public int VarcharLength { get; set; }
        public EnumAsVarcharAttribute(int varcharLength = 32)
        {
            this.VarcharLength = varcharLength;
        }
    }
}

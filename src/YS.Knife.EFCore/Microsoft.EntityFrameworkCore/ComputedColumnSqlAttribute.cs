namespace Microsoft.EntityFrameworkCore
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    public sealed class ComputedColumnSqlAttribute : ProviderAttribute
    {
        public ComputedColumnSqlAttribute(string sql, bool stored)
        {
            this.Sql = sql;
            this.Stored = stored;
        }
        public string Sql { get; }
        public bool Stored { get; }
    }
}

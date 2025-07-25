namespace Microsoft.EntityFrameworkCore
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    public sealed class DefaultValueSqlAttribute : ProviderAttribute
    {
        public DefaultValueSqlAttribute(string sql)
        {
            Sql = sql;
        }
        public string Sql { get; set; }
    }
}

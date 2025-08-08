using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    public sealed class DefaultValueSqlAttribute : ProviderAttribute, IModelPropertyAttribute
    {
        public DefaultValueSqlAttribute(string sql)
        {
            Sql = sql;
        }
        public string Sql { get; set; }

        public void Apply(IMutableProperty property)
        {
            property.SetDefaultValueSql(Sql);
        }
    }

}

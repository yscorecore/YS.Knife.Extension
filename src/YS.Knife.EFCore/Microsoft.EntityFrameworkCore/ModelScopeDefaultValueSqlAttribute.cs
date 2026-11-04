using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public class ModelScopeDefaultValueSqlAttribute : ProviderAttribute, IModelAttribute
    {
        public ModelScopeDefaultValueSqlAttribute(string propertyName, Type propertyType, string sql)
        {
            PropertyName = propertyName;
            PropertyType = propertyType;
            Sql = sql;
        }

        public string PropertyName { get; }
        public Type PropertyType { get; }
        public string Sql { get; set; }
        public void Apply(ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
#if NET8_0_OR_GREATER
                // EF Core 8 moved FindProperty(string) to IReadOnlyTypeBase.
                var idProperty = (IMutableProperty?)((IReadOnlyTypeBase)entityType).FindProperty(this.PropertyName);
#else
                var idProperty = entityType.FindProperty(this.PropertyName);
#endif
                if (idProperty != null && idProperty.ClrType == PropertyType)
                {
                    idProperty.SetDefaultValueSql(Sql);
                }
            }
        }
    }
}

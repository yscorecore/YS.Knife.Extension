using Microsoft.EntityFrameworkCore;

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
                var idProperty = entityType.FindProperty(this.PropertyName);
                if (idProperty != null && idProperty.ClrType == PropertyType)
                {
                    idProperty.SetDefaultValueSql(Sql);
                }
            }
        }
    }
}

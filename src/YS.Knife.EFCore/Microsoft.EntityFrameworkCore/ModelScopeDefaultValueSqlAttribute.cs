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
                // 通过 IReadOnlyEntityType.FindProperty 查找属性，兼容 EF Core 6~10。
                // EF Core 10 将 FindProperty(string) 从 IMutableEntityType 接口移到了父接口 IMutableTypeBase，
                // 直接在 IMutableEntityType 变量上调用会生成对 IMutableEntityType.FindProperty 的二进制引用，
                // 在 EF Core 10 运行时找不到 → MissingMethodException。
                // IReadOnlyEntityType.FindProperty 在所有版本都可编译（EF Core 6 直接声明，EF Core 10 继承自 IReadOnlyTypeBase），
                // 返回的 IReadOnlyProperty 运行时实际类型是可写实现，可安全 cast 回 IMutableProperty。
                var idProperty = (IMutableProperty?)((IReadOnlyEntityType)entityType).FindProperty(this.PropertyName);
                if (idProperty != null && idProperty.ClrType == PropertyType)
                {
                    idProperty.SetDefaultValueSql(Sql);
                }
            }
        }
    }
}

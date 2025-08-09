using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microsoft.EntityFrameworkCore
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class CheckConstraintSqlAttribute : ProviderAttribute, IModelTypeAttribute
    {
        public CheckConstraintSqlAttribute(string constraintName, string sql)
        {
            ConstraintName = constraintName;
            Sql = sql;
        }
        public string ConstraintName { get; set; }
        public string Sql { get; set; }

        public void Apply(EntityTypeBuilder typeBuilder)
        {
            typeBuilder.HasCheckConstraint($"{typeBuilder.Metadata?.ClrType?.Name}_{ConstraintName}", Sql);
        }
    }
}

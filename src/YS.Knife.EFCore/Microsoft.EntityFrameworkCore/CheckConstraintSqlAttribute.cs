using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata;

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

        public void Apply(IMutableEntityType type)
        {
            var current = type.FindCheckConstraint(ConstraintName);
            if (current != null)
            {
                type.RemoveCheckConstraint(ConstraintName);
            }
            type.AddCheckConstraint(ConstraintName, Sql);
        }
    }
}

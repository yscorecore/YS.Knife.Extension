using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EntityFrameworkCore
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class CheckConstraintSqlAttribute : ProviderAttribute
    {
        public CheckConstraintSqlAttribute(string constraintName, string sql)
        {
            ConstraintName = constraintName;
            Sql = sql;
        }
        public string ConstraintName { get; set; }
        public string Sql { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace YS.Knife.DataSource
{
    public record DataSourceEntry
    {
        public string Name { get; set; }
        public Type ServiceType { get; set; }
        public MethodInfo Method { get; set; }
        public object[] Arguments { get; set; }
        public Type EntityType { get; set; }
        public bool IsValueTask { get; set; }
        public bool HasCancellationToken { get; set; }
        public bool AutoRegisterMeta { get; set; }

    }
}

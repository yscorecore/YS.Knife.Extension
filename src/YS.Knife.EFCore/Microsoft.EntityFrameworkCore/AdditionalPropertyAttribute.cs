using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YS.Knife.EfCore.Microsoft.EntityFrameworkCore
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class AdditionalPropertyAttribute : Attribute
    {
        public AdditionalPropertyAttribute(Type type, string propertyName)
        {
            Type = type;
            PropertyName = propertyName;
        }

        public Type Type { get; }
        public string PropertyName { get; }
    }
}

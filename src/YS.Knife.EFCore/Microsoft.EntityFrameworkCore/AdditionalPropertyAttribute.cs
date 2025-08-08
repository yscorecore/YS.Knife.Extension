using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class AdditionalPropertyAttribute : Attribute, IModelTypeAttribute, IModelAttribute
    {
        public AdditionalPropertyAttribute(Type type, string propertyName)
        {
            Type = type;
            PropertyName = propertyName;
        }

        public Type Type { get; }
        public string PropertyName { get; }

        public void Apply(IMutableEntityType type)
        {
            //throw new NotImplementedException();
            // type.AddProperty(PropertyName, Type, ConfigurationSource.Convention);
        }

        public void Apply(IMutableModel model)
        {

            // throw new NotImplementedException();
        }
    }
}

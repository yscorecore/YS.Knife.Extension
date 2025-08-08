using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore
{
    public abstract class PropertyAttribute : Attribute
    {
    }
    public interface IModelPropertyAttribute
    {
        void Apply(IMutableProperty property);

    }
    public interface IModelAttribute
    {
        void Apply(IMutableModel model);
    }
    public interface IModelTypeAttribute
    {
        void Apply(IMutableEntityType type);
    }
}

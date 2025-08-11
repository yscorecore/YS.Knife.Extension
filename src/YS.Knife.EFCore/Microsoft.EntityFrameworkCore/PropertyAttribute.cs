using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microsoft.EntityFrameworkCore
{
    public abstract class PropertyAttribute : Attribute
    {
    }
    public interface IModelPropertyAttribute
    {
        void Apply(PropertyBuilder propertyBuilder);
    }
    public interface IModelAttribute
    {
        void Apply(ModelBuilder modelBuilder);
    }
    public interface IModelTypeAttribute
    {
        void Apply(EntityTypeBuilder typeBuilder);
    }
    public interface IModelMethodAttribute
    {
        void Apply(ModelBuilder modelBuilder, MethodInfo methodInfo);
    }
}

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Microsoft.EntityFrameworkCore
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class EnumAsVarcharAttribute : Attribute, IModelPropertyAttribute
    {
        public int VarcharLength { get; set; }
        public EnumAsVarcharAttribute(int varcharLength = 32)
        {
            this.VarcharLength = varcharLength;
        }
        public void Apply(PropertyBuilder property)
        {
            if (property.Metadata.ClrType.IsEnum == false)
            {
                throw new InvalidOperationException($"The property '{property.Metadata.Name}' is not an enum type.");

            }
            var mappintHints = new ConverterMappingHints(size: VarcharLength);
            var type = typeof(EnumToStringConverter<>).MakeGenericType(property.Metadata.ClrType);
            var instance = Activator.CreateInstance(type, new object[] { mappintHints }) as ValueConverter;
            property.HasConversion(instance);
        }
    }
}

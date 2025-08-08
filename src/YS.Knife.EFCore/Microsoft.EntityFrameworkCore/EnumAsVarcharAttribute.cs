using Microsoft.EntityFrameworkCore.Metadata;
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

        public void Apply(IMutableProperty property)
        {
            if (property.ClrType.IsEnum == false)
            {
                throw new InvalidOperationException($"The property '{property.Name}' is not an enum type.");
            }
            var mappintHints = new ConverterMappingHints(size: VarcharLength);
            var type = typeof(EnumToStringConverter<>).MakeGenericType(property.ClrType);
            var instance = Activator.CreateInstance(type, new object[] { mappintHints }) as ValueConverter;
            property.SetValueConverter(instance);
        }
    }
}

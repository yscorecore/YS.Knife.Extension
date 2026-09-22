using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Microsoft.EntityFrameworkCore
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class EncryptedAttribute : Attribute, IModelPropertyAttribute
    {
        private readonly Type providerType;
        private readonly object[] providerArgs;

        public string Prefix { get; set; } = "enc:";

        public EncryptedAttribute(Type providerType, params object[] args)
        {
            if (!typeof(IEncryptionProvider).IsAssignableFrom(providerType))
                throw new ArgumentException(
                    $"{providerType.Name} must implement IEncryptionProvider.", nameof(providerType));
            this.providerType = providerType;
            this.providerArgs = args;
        }

        public void Apply(PropertyBuilder property)
        {
            var clrType = property.Metadata.ClrType;
            var underlyingType = Nullable.GetUnderlyingType(clrType) ?? clrType;
            if (underlyingType != typeof(string))
            {
                throw new InvalidOperationException(
                    $"The property '{property.Metadata.Name}' must be of type 'string' to use [Encrypted].");
            }

            var provider = (IEncryptionProvider)Activator.CreateInstance(providerType, providerArgs);

            var maxLength = property.Metadata.GetMaxLength() ?? 256;
            var encryptedSize = Prefix.Length + provider.GetEncryptedLength(maxLength);
            var hints = new ConverterMappingHints(size: encryptedSize);

            var converter = new EncryptedValueConvert(provider, Prefix, hints);
            property.HasConversion(converter);
        }

        private class EncryptedValueConvert : ValueConverter<string, string>
        {
            public EncryptedValueConvert(IEncryptionProvider provider, string prefix, ConverterMappingHints hints)
                : base(
                    v => v == null ? null : prefix + provider.Encrypt(v),
                    v => string.IsNullOrEmpty(v) || !v.StartsWith(prefix)
                        ? v
                        : provider.Decrypt(v.Substring(prefix.Length)),
                    hints)
            {
            }
        }
    }
}

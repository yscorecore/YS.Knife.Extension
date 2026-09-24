using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Microsoft.EntityFrameworkCore
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class SymmetricEncryptionAttribute : Attribute, IModelPropertyAttribute
    {
        private readonly string algorithmName;

        public string KeyName { get; }

        public string Prefix { get; set; } = "enc:";

        public SymmetricEncryptionAttribute(string algorithmName, string keyName)
        {
            this.algorithmName = algorithmName;
            KeyName = keyName;
        }

        public void Apply(PropertyBuilder property)
        {
            var clrType = property.Metadata.ClrType;
            var underlyingType = Nullable.GetUnderlyingType(clrType) ?? clrType;
            if (underlyingType != typeof(string))
            {
                throw new InvalidOperationException(
                    $"The property '{property.Metadata.Name}' must be of type 'string' to use [SymmetricEncryption].");
            }

            var provider = new SymmetricEncryptionProvider(algorithmName, KeyName);

            var maxLength = property.Metadata.GetMaxLength() ?? 256;
            var encryptedSize = Prefix.Length + provider.GetEncryptedLength(maxLength);
            var hints = new ConverterMappingHints(size: encryptedSize);
            property.HasMaxLength(encryptedSize);
            var converter = new SymmetricEncryptionValueConvert(provider, Prefix, hints);
            property.HasConversion(converter);
        }

        private class SymmetricEncryptionValueConvert : ValueConverter<string, string>
        {
            public SymmetricEncryptionValueConvert(IEncryptionProvider provider, string prefix, ConverterMappingHints hints)
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

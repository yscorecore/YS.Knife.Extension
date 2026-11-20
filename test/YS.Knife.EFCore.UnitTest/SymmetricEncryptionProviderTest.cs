using Microsoft.EntityFrameworkCore;

namespace YS.Knife.EFCore.UnitTest
{
    public class SymmetricEncryptionProviderTest
    {
        [Fact]
        public void Should_EncryptAndDecrypt_When_KeySetDirectly()
        {
            var provider = new SymmetricEncryptionProvider("AES", "unused-key-name");
            provider.SetKey("my-secret-passphrase");

            var plainText = "Hello, World!";
            var encrypted = provider.Encrypt(plainText);
            var decrypted = provider.Decrypt(encrypted);

            decrypted.Should().Be(plainText);
        }

        [Fact]
        public void Should_EncryptAndDecrypt_When_KeyRegisteredInEncryptionKeys()
        {
            EncryptionKeys.Set("test-aes-key", "another-passphrase");
            var provider = new SymmetricEncryptionProvider("AES", "test-aes-key");

            var plainText = "Sensitive data here";
            var encrypted = provider.Encrypt(plainText);
            var decrypted = provider.Decrypt(encrypted);

            decrypted.Should().Be(plainText);
        }

        [Fact]
        public void Should_ProduceDifferentCiphertext_When_EncryptSameTextTwice()
        {
            var provider = new SymmetricEncryptionProvider("AES", "unused");
            provider.SetKey("passphrase");

            var plainText = "Same text";
            var encrypted1 = provider.Encrypt(plainText);
            var encrypted2 = provider.Encrypt(plainText);

            encrypted1.Should().NotBe(encrypted2);
        }

        [Fact]
        public void Should_ThrowInvalidOperationException_When_KeyNotSetAndNotRegistered()
        {
            var provider = new SymmetricEncryptionProvider("AES", "non-existent-key-xyz");

            var act = () => provider.Encrypt("test");

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*non-existent-key-xyz*");
        }

        [Fact]
        public void Should_ThrowInvalidOperationException_OnDecrypt_When_KeyNotSetAndNotRegistered()
        {
            var provider = new SymmetricEncryptionProvider("AES", "non-existent-key-abc");

            var act = () => provider.Decrypt("dGVzdA==");

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*non-existent-key-abc*");
        }

        [Fact]
        public void Should_ThrowArgumentException_When_UnsupportedAlgorithm()
        {
            var act = () => new SymmetricEncryptionProvider("NonExistentAlg", "key");

            act.Should().Throw<ArgumentException>()
                .WithMessage("*NonExistentAlg*");
        }

        [Fact]
        public void Should_ReturnPositiveLength_When_GetEncryptedLength()
        {
            var provider = new SymmetricEncryptionProvider("AES", "unused");
            provider.SetKey("passphrase");

            var length = provider.GetEncryptedLength(256);

            length.Should().BeGreaterThan(0);
        }

        [Fact]
        public void Should_HandleEmptyString()
        {
            var provider = new SymmetricEncryptionProvider("AES", "unused");
            provider.SetKey("passphrase");

            var encrypted = provider.Encrypt("");
            var decrypted = provider.Decrypt(encrypted);

            decrypted.Should().BeEmpty();
        }

        [Fact]
        public void Should_HandleUnicodeText()
        {
            var provider = new SymmetricEncryptionProvider("AES", "unused");
            provider.SetKey("passphrase");

            var plainText = "你好世界！こんにちは世界！";
            var encrypted = provider.Encrypt(plainText);
            var decrypted = provider.Decrypt(encrypted);

            decrypted.Should().Be(plainText);
        }
    }
}

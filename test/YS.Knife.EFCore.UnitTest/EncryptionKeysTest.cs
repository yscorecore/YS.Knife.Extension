using Microsoft.EntityFrameworkCore;

namespace YS.Knife.EFCore.UnitTest
{
    public class EncryptionKeysTest
    {
        [Fact]
        public void Should_GetKey_When_SetBefore()
        {
            EncryptionKeys.Set("test-key-1", "secret-value-1");

            var result = EncryptionKeys.Get("test-key-1");

            result.Should().Be("secret-value-1");
        }

        [Fact]
        public void Should_ThrowKeyNotFoundException_When_GetNonExistentKey()
        {
            var act = () => EncryptionKeys.Get("definitely-not-exists-key-999");

            act.Should().Throw<KeyNotFoundException>();
        }

        [Fact]
        public void Should_ReturnTrue_When_TryGetExistingKey()
        {
            EncryptionKeys.Set("test-key-2", "secret-value-2");

            var found = EncryptionKeys.TryGet("test-key-2", out var key);

            found.Should().BeTrue();
            key.Should().Be("secret-value-2");
        }

        [Fact]
        public void Should_ReturnFalse_When_TryGetNonExistentKey()
        {
            var found = EncryptionKeys.TryGet("definitely-not-exists-key-888", out var key);

            found.Should().BeFalse();
            key.Should().BeNull();
        }

        [Fact]
        public void Should_OverwriteKey_When_SetSameNameTwice()
        {
            EncryptionKeys.Set("overwrite-key", "first-value");
            EncryptionKeys.Set("overwrite-key", "second-value");

            var result = EncryptionKeys.Get("overwrite-key");

            result.Should().Be("second-value");
        }
    }
}

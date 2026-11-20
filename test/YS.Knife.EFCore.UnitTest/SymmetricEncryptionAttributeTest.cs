using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit.Abstractions;

namespace YS.Knife.EFCore.UnitTest
{
    [AutoConstructor]
    public partial class SymmetricEncryptionAttributeTest : BaseTest
    {
        [Fact]
        public void Should_EncryptAndDecrypt_When_SaveAndRead()
        {
            EncryptionKeys.Set("ef-test-key", "test-passphrase");
            var options = new DbContextOptionsBuilder<SymmetricEncryption_Context>();
            options.UseSqlite("Data Source=:memory:").EnableSensitiveDataLogging();
            using var context = new SymmetricEncryption_Context(options.Options);
            context.Database.OpenConnection();
            context.Database.EnsureCreated();

            context.TestEntities.Add(new SymmetricEncryption_Entity { Id = 1, SecretData = "Hello Secret" });
            context.SaveChanges();

            using var cmd = context.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = "SELECT SecretData FROM TestEntities WHERE Id = 1";
            using var reader = cmd.ExecuteReader();
            reader.Read();
            var raw = reader.GetString(0);
            raw.Should().StartWith("enc:");

            var entity = context.TestEntities.First();
            entity.SecretData.Should().Be("Hello Secret");
        }

        [Fact]
        public void Should_ReturnRawValue_When_DataNotEncrypted()
        {
            EncryptionKeys.Set("ef-test-key", "test-passphrase");
            var options = new DbContextOptionsBuilder<SymmetricEncryption_Context>();
            options.UseSqlite("Data Source=:memory:").EnableSensitiveDataLogging();
            using var context = new SymmetricEncryption_Context(options.Options);
            context.Database.OpenConnection();
            context.Database.EnsureCreated();

            context.Database.ExecuteSqlRaw("INSERT INTO TestEntities (Id, SecretData) VALUES (1, 'plain-text')");

            var entity = context.TestEntities.First();
            entity.SecretData.Should().Be("plain-text");
        }

        private class SymmetricEncryption_Context : BaseContext
        {
            public SymmetricEncryption_Context(DbContextOptions<SymmetricEncryption_Context> options) : base(options)
            {
            }
            public DbSet<SymmetricEncryption_Entity> TestEntities { get; set; }
        }

        private class SymmetricEncryption_Entity
        {
            public int Id { get; set; }
            [SymmetricEncryption("AES", "ef-test-key")]
            public string SecretData { get; set; }
        }
    }
}

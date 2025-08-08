using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace YS.Knife.EFCore.UnitTest
{
    [AutoConstructor]
    public partial class DefaultValueSqlAttributeTest : BaseTest
    {
        #region GetDefaultValueSql_When_DefineDefaultValueAttribute
        [Fact]
        public void Should_GetDefaultValueSql_When_DefineDefaultValueAttribute()
        {
            var model = GetModel<GetDefaultValueSql_When_DefineDefaultValueAttribute_Context>();
            var entity = model.GetEntityTypes().First();
            var property = entity.GetProperty(nameof(GetDefaultValueSql_When_DefineDefaultValueAttribute_Context_Entity.Value));
            property.GetDefaultValueSql().Should().Be("CURRENT_TIMESTAMP");
        }

        private class GetDefaultValueSql_When_DefineDefaultValueAttribute_Context : BaseContext
        {
            public GetDefaultValueSql_When_DefineDefaultValueAttribute_Context(DbContextOptions<GetDefaultValueSql_When_DefineDefaultValueAttribute_Context> options) : base(options)
            {
            }
            public DbSet<GetDefaultValueSql_When_DefineDefaultValueAttribute_Context_Entity> TestEntities { get; set; }
        }

        private class GetDefaultValueSql_When_DefineDefaultValueAttribute_Context_Entity
        {
            public int Id { get; set; }
            [DefaultValueSql("CURRENT_TIMESTAMP")]
            public DateTime Value { get; set; }
        }
        #endregion

        #region GetDefaultValueSql_When_Define_DefaultValueAttribute_And_DefaultValueAttributeWithProvider
        [Fact]
        public void Should_GetDefaultValueSql_When_Define_DefaultValueAttribute_And_DefaultValueAttributeWithProvider()
        {
            var model = GetModel<GetDefaultValueSql_When_Define_DefaultValueAttribute_And_DefaultValueAttributeWithProvider_Context>();
            var entity = model.GetEntityTypes().First();
            var property = entity.GetProperty(nameof(GetDefaultValueSql_When_Define_DefaultValueAttribute_And_DefaultValueAttributeWithProvider_Entity.Value));
            property.GetDefaultValueSql().Should().Be("now()");
        }

        private class GetDefaultValueSql_When_Define_DefaultValueAttribute_And_DefaultValueAttributeWithProvider_Context : BaseContext
        {
            public GetDefaultValueSql_When_Define_DefaultValueAttribute_And_DefaultValueAttributeWithProvider_Context(DbContextOptions<GetDefaultValueSql_When_Define_DefaultValueAttribute_And_DefaultValueAttributeWithProvider_Context> options) : base(options)
            {
            }
            public DbSet<GetDefaultValueSql_When_Define_DefaultValueAttribute_And_DefaultValueAttributeWithProvider_Entity> TestEntities { get; set; }
        }

        private class GetDefaultValueSql_When_Define_DefaultValueAttribute_And_DefaultValueAttributeWithProvider_Entity
        {
            public int Id { get; set; }
            [DefaultValueSql("now()", Provider = "Microsoft.EntityFrameworkCore.Sqlite")]
            [DefaultValueSql("CURRENT_TIMESTAMP")]
            public DateTime Value { get; set; }
        }
        #endregion

    }
}

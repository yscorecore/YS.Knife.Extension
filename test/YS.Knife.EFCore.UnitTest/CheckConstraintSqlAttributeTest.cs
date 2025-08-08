using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace YS.Knife.EFCore.UnitTest
{
    [AutoConstructor]
    public partial class CheckConstraintSqlAttributeTest : BaseTest
    {

        #region GetCheckConstraintSql_When_DefineDefaultValueAttribute
        [Fact]
        public void Should_GetCheckConstraintSql_When_DefineDefaultValueAttribute()
        {
            var model = GetModel<GetCheckConstraintSql_When_DefineDefaultValueAttribute_Context>();
            var entity = model.GetEntityTypes().First();
            entity.GetCheckConstraints().Count().Should().Be(1);
            var checkConstanint = entity.GetCheckConstraints().First();
            checkConstanint.Sql.Should().Be("Value >5");
            checkConstanint.Name.Should().Be("check1");

        }

        private class GetCheckConstraintSql_When_DefineDefaultValueAttribute_Context : BaseContext
        {
            public GetCheckConstraintSql_When_DefineDefaultValueAttribute_Context(DbContextOptions<GetCheckConstraintSql_When_DefineDefaultValueAttribute_Context> options) : base(options)
            {
            }
            public DbSet<GetCheckConstraintSql_When_DefineDefaultValueAttribute_Context_Entity> TestEntities { get; set; }
        }
        [CheckConstraintSql("check1", "Value >5")]
        private class GetCheckConstraintSql_When_DefineDefaultValueAttribute_Context_Entity
        {
            public int Id { get; set; }
            public int Value { get; set; }
        }
        #endregion

        #region GetCheckConstraintSql_When_Define_DefaultValueAttribute_And_DefaultValueAttributeWithProvider
        [Fact]
        public void Should_GetCheckConstraintSql_When_Define_DefaultValueAttribute_And_DefaultValueAttributeWithProvider()
        {
            var model = GetModel<GetCheckConstraintSql_When_Define_DefaultValueAttribute_And_DefaultValueAttributeWithProvider_Context>();
            var entity = model.GetEntityTypes().First();
            var checkConstanint = entity.GetCheckConstraints().First();
            checkConstanint.Sql.Should().Be("Value > 6");
            checkConstanint.Name.Should().Be("check1");
        }

        private class GetCheckConstraintSql_When_Define_DefaultValueAttribute_And_DefaultValueAttributeWithProvider_Context : BaseContext
        {
            public GetCheckConstraintSql_When_Define_DefaultValueAttribute_And_DefaultValueAttributeWithProvider_Context(DbContextOptions<GetCheckConstraintSql_When_Define_DefaultValueAttribute_And_DefaultValueAttributeWithProvider_Context> options) : base(options)
            {
            }
            public DbSet<GetCheckConstraintSql_When_Define_DefaultValueAttribute_And_DefaultValueAttributeWithProvider_Entity> TestEntities { get; set; }
        }
        [CheckConstraintSql("check1", "Value > 6", Provider = "Microsoft.EntityFrameworkCore.Sqlite")]
        [CheckConstraintSql("check1", "Value >5")]
        private class GetCheckConstraintSql_When_Define_DefaultValueAttribute_And_DefaultValueAttributeWithProvider_Entity
        {
            public int Id { get; set; }
            public int Value { get; set; }

            [JsonContent]
            public Dictionary<string, object> Details { get; set; }
        }
        #endregion


    }
}

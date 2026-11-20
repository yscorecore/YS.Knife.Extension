using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace YS.Knife.EFCore.UnitTest
{
    public class UniqueIndexInterceptorTest
    {
        private static UniqueTestContext CreateContext()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            connection.Open();
            var options = new DbContextOptionsBuilder<UniqueTestContext>()
                .UseSqlite(connection)
                .AddInterceptors(new UniqueIndexInterceptor())
                .Options;
            var context = new UniqueTestContext(options, connection);
            context.Database.EnsureCreated();
            return context;
        }

        [Fact]
        public void Should_Not_Throw_When_Unique_Value_Is_New()
        {
            using var context = CreateContext();
            context.GlobalEntities.Add(new GlobalUniqueEntity { Name = "A" });
            context.SaveChanges();

            context.GlobalEntities.Add(new GlobalUniqueEntity { Name = "B" });
            var act = () => context.SaveChanges();

            act.Should().NotThrow();
        }

        [Fact]
        public void Should_Throw_When_Duplicate_Value_Exists_In_Database()
        {
            using var context = CreateContext();
            context.GlobalEntities.Add(new GlobalUniqueEntity { Name = "A" });
            context.SaveChanges();

            context.GlobalEntities.Add(new GlobalUniqueEntity { Name = "A" });
            var act = () => context.SaveChanges();

            act.Should().Throw<ValidationException>().WithMessage("Name 重复");
        }

        [Fact]
        public void Should_Throw_When_Duplicates_Added_In_Same_Batch()
        {
            using var context = CreateContext();
            context.GlobalEntities.Add(new GlobalUniqueEntity { Name = "Dup" });
            context.GlobalEntities.Add(new GlobalUniqueEntity { Name = "Dup" });
            var act = () => context.SaveChanges();

            act.Should().Throw<ValidationException>().WithMessage("Name 重复");
        }

        [Fact]
        public void Should_Skip_When_Index_Value_Is_Null()
        {
            using var context = CreateContext();
            context.GlobalEntities.Add(new GlobalUniqueEntity { Name = null });
            context.GlobalEntities.Add(new GlobalUniqueEntity { Name = null });
            var act = () => context.SaveChanges();

            act.Should().NotThrow();
        }

        [Fact]
        public void Should_Throw_When_Composite_Index_Duplicated()
        {
            using var context = CreateContext();
            context.ScopedEntities.Add(new ScopedUniqueEntity { Name = "A", Scope = "S1" });
            context.SaveChanges();

            context.ScopedEntities.Add(new ScopedUniqueEntity { Name = "A", Scope = "S1" });
            var act = () => context.SaveChanges();

            act.Should().Throw<ValidationException>().WithMessage("Scope 重复");
        }

        [Fact]
        public void Should_Not_Throw_When_Composite_Index_Part_Differs()
        {
            using var context = CreateContext();
            context.ScopedEntities.Add(new ScopedUniqueEntity { Name = "A", Scope = "S1" });
            context.SaveChanges();

            context.ScopedEntities.Add(new ScopedUniqueEntity { Name = "A", Scope = "S2" });
            var act = () => context.SaveChanges();

            act.Should().NotThrow();
        }

        [Fact]
        public void Should_Not_Check_When_Index_Field_Not_Modified()
        {
            using var context = CreateContext();
            context.GlobalEntities.Add(new GlobalUniqueEntity { Name = "A", Remark = "r1" });
            context.SaveChanges();

            var entity = context.GlobalEntities.Single();
            entity.Remark = "r2";
            var act = () => context.SaveChanges();

            act.Should().NotThrow();
        }

        [Fact]
        public void Should_Throw_When_Updated_To_Existing_Value()
        {
            using var context = CreateContext();
            context.GlobalEntities.Add(new GlobalUniqueEntity { Name = "A" });
            context.GlobalEntities.Add(new GlobalUniqueEntity { Name = "B" });
            context.SaveChanges();

            var entity = context.GlobalEntities.Single(p => p.Name == "B");
            entity.Name = "A";
            var act = () => context.SaveChanges();

            act.Should().Throw<ValidationException>().WithMessage("Name 重复");
        }

        [Fact]
        public void Should_Use_Description_In_Error_Message()
        {
            using var context = CreateContext();
            context.DescriptionEntities.Add(new DescriptionEntity { Name = "A" });
            context.SaveChanges();

            context.DescriptionEntities.Add(new DescriptionEntity { Name = "A" });
            var act = () => context.SaveChanges();

            act.Should().Throw<ValidationException>().WithMessage("名称 重复");
        }

        [Fact]
        public void Should_Use_Comment_In_Error_Message_When_No_Description()
        {
            using var context = CreateContext();
            context.CommentEntities.Add(new CommentEntity { Code = "C1" });
            context.SaveChanges();

            context.CommentEntities.Add(new CommentEntity { Code = "C1" });
            var act = () => context.SaveChanges();

            act.Should().Throw<ValidationException>().WithMessage("编码 重复");
        }

        [Fact]
        public void Should_Ignore_Non_Unique_Index()
        {
            using var context = CreateContext();
            context.NonUniqueEntities.Add(new NonUniqueIndexEntity { Name = "A" });
            context.NonUniqueEntities.Add(new NonUniqueIndexEntity { Name = "A" });
            var act = () => context.SaveChanges();

            act.Should().NotThrow();
        }

        private class UniqueTestContext : BaseContext
        {
            private readonly SqliteConnection connection;

            public UniqueTestContext(DbContextOptions<UniqueTestContext> options, SqliteConnection connection) : base(options)
            {
                this.connection = connection;
            }

            public override void Dispose()
            {
                base.Dispose();
                this.connection.Dispose();
            }

            public DbSet<GlobalUniqueEntity> GlobalEntities { get; set; }
            public DbSet<ScopedUniqueEntity> ScopedEntities { get; set; }
            public DbSet<DescriptionEntity> DescriptionEntities { get; set; }
            public DbSet<CommentEntity> CommentEntities { get; set; }
            public DbSet<NonUniqueIndexEntity> NonUniqueEntities { get; set; }
        }

        [Index(nameof(Name), IsUnique = true)]
        private class GlobalUniqueEntity
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public string Remark { get; set; }
        }

        [Index(nameof(Name), nameof(Scope), IsUnique = true)]
        private class ScopedUniqueEntity
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public string Scope { get; set; }
        }

        [Index(nameof(Name), IsUnique = true)]
        private class DescriptionEntity
        {
            public int Id { get; set; }

            [Description("名称")]
            public string Name { get; set; }
        }

        [Index(nameof(Code), IsUnique = true)]
        private class CommentEntity
        {
            public int Id { get; set; }

            [Comment("编码")]
            public string Code { get; set; }
        }

        [Index(nameof(Name))]
        private class NonUniqueIndexEntity
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }
    }
}

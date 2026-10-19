using System.ComponentModel.DataAnnotations;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace YS.Knife.EFCore.UnitTest
{
    public class UniqueNameInterceptorTest
    {
        private static UniqueTestContext CreateContext()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            connection.Open();
            var options = new DbContextOptionsBuilder<UniqueTestContext>()
                .UseSqlite(connection)
                .AddInterceptors(new UniqueNameInterceptor())
                .Options;
            var context = new UniqueTestContext(options, connection);
            context.Database.EnsureCreated();
            return context;
        }

        [Fact]
        public void Should_Not_Throw_When_Global_Name_Is_Unique()
        {
            using var context = CreateContext();
            context.GlobalEntities.Add(new GlobalUniqueEntity { Name = "A" });
            context.SaveChanges();

            context.GlobalEntities.Add(new GlobalUniqueEntity { Name = "B" });
            var act = () => context.SaveChanges();

            act.Should().NotThrow();
        }

        [Fact]
        public void Should_Throw_When_Global_Name_Duplicates_Existing_Row()
        {
            using var context = CreateContext();
            context.GlobalEntities.Add(new GlobalUniqueEntity { Name = "A" });
            context.SaveChanges();

            context.GlobalEntities.Add(new GlobalUniqueEntity { Name = "A" });
            var act = () => context.SaveChanges();

            act.Should().Throw<ValidationException>().WithMessage("*'A'*");
        }

        [Fact]
        public void Should_Throw_When_Two_Duplicates_Added_In_Same_Batch()
        {
            using var context = CreateContext();
            context.GlobalEntities.Add(new GlobalUniqueEntity { Name = "Dup" });
            context.GlobalEntities.Add(new GlobalUniqueEntity { Name = "Dup" });
            var act = () => context.SaveChanges();

            act.Should().Throw<ValidationException>().WithMessage("*'Dup'*");
        }

        [Fact]
        public void Should_Not_Validate_When_Unique_Value_Is_Null()
        {
            using var context = CreateContext();
            context.GlobalEntities.Add(new GlobalUniqueEntity { Name = null });
            context.GlobalEntities.Add(new GlobalUniqueEntity { Name = null });
            var act = () => context.SaveChanges();

            act.Should().NotThrow();
        }

        [Fact]
        public void Should_Throw_When_Duplicate_Within_Same_Scope()
        {
            using var context = CreateContext();
            context.ScopedEntities.Add(new ScopedUniqueEntity { Name = "A", Scope = "S1" });
            context.SaveChanges();

            context.ScopedEntities.Add(new ScopedUniqueEntity { Name = "A", Scope = "S1" });
            var act = () => context.SaveChanges();

            act.Should().Throw<ValidationException>().WithMessage("*'A'*");
        }

        [Fact]
        public void Should_Not_Throw_When_Same_Name_In_Different_Scope()
        {
            using var context = CreateContext();
            context.ScopedEntities.Add(new ScopedUniqueEntity { Name = "A", Scope = "S1" });
            context.SaveChanges();

            context.ScopedEntities.Add(new ScopedUniqueEntity { Name = "A", Scope = "S2" });
            var act = () => context.SaveChanges();

            act.Should().NotThrow();
        }

        [Fact]
        public void Should_Throw_When_IgnoreCase_And_Different_Case()
        {
            using var context = CreateContext();
            context.IgnoreCaseEntities.Add(new IgnoreCaseEntity { Name = "ABC" });
            context.SaveChanges();

            context.IgnoreCaseEntities.Add(new IgnoreCaseEntity { Name = "abc" });
            var act = () => context.SaveChanges();

            act.Should().Throw<ValidationException>();
        }

        [Fact]
        public void Should_Not_Check_When_Uniqueness_Field_Not_Modified()
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
        public void Should_Throw_When_Update_To_Existing_Value()
        {
            using var context = CreateContext();
            context.GlobalEntities.Add(new GlobalUniqueEntity { Name = "A" });
            context.GlobalEntities.Add(new GlobalUniqueEntity { Name = "B" });
            context.SaveChanges();

            var entity = context.GlobalEntities.Single(p => p.Name == "B");
            entity.Name = "A";
            var act = () => context.SaveChanges();

            act.Should().Throw<ValidationException>().WithMessage("*'A'*");
        }

        [Fact]
        public void Should_Use_Custom_ErrorMessage()
        {
            using var context = CreateContext();
            context.CustomMessageEntities.Add(new CustomMessageEntity { Name = "A" });
            context.SaveChanges();

            context.CustomMessageEntities.Add(new CustomMessageEntity { Name = "A" });
            var act = () => context.SaveChanges();

            act.Should().Throw<ValidationException>().WithMessage("唯一冲突:Name=A");
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
            public DbSet<IgnoreCaseEntity> IgnoreCaseEntities { get; set; }
            public DbSet<CustomMessageEntity> CustomMessageEntities { get; set; }
        }

        private class GlobalUniqueEntity
        {
            public int Id { get; set; }

            [UniqueName]
            public string Name { get; set; }

            public string Remark { get; set; }
        }

        private class ScopedUniqueEntity
        {
            public int Id { get; set; }

            [UniqueName(nameof(Scope))]
            public string Name { get; set; }

            public string Scope { get; set; }
        }

        private class IgnoreCaseEntity
        {
            public int Id { get; set; }

            [UniqueName(IgnoreCase = true)]
            public string Name { get; set; }
        }

        private class CustomMessageEntity
        {
            public int Id { get; set; }

            [UniqueName(ErrorMessage = "唯一冲突:{0}={1}")]
            public string Name { get; set; }
        }
    }
}

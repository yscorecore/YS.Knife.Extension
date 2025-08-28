using Microsoft.EntityFrameworkCore;
using YS.Knife.Entity;

namespace TagsDemo
{
    [ModelScopeDefaultValueSql(nameof(YS.Knife.Entity.BaseEntity<Guid>.CreateTime), typeof(DateTime), "current_timestamp")]

    public class TagDbContext : DbContext
    {
        public TagDbContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<SchoolEntity> Schools { get; set; }
        public DbSet<SchoolTag> SchoolTags { get; set; }
        public DbSet<UserEntity> Users { get; set; }
        public DbSet<UserTag> UserTags { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            this.ApplyKnifeExtensions(modelBuilder);
        }
    }

    public class SchoolEntity : BaseEntity<Guid>, ITagOwnerEntity<SchoolEntity, SchoolTag, Guid>
    {
        public string Name { get; set; }
        public List<SchoolTag> Tags { get; set; } = new();
    }
    public class SchoolTag : Tag<SchoolEntity, Guid> { }
    public class UserTag : Tag<UserEntity, Guid> { }
    public class UserEntity : BaseEntity<Guid>, ITagOwnerEntity<UserEntity, UserTag, Guid>
    {
        public string Name { get; set; }
        public List<UserTag> Tags { get; set; } = new();
    }

}

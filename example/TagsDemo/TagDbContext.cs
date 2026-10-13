using Microsoft.EntityFrameworkCore;
using YS.Knife.Entity;

namespace TagsDemo
{
    [ModelScopeDefaultValueSql(nameof(YS.Knife.Entity.FullAuditedEntity<Guid>.CreationTime), typeof(DateTimeOffset), "current_timestamp")]

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

    public class SchoolEntity : FullAuditedEntity<Guid>, ITagOwnerEntity<SchoolEntity, SchoolTag, Guid>
    {
        public string Name { get; set; }
        public List<SchoolTag> Tags { get; set; } = new();
    }
    public class SchoolTag : Tag<SchoolEntity, Guid> { }
    public class UserTag : Tag<UserEntity, Guid> { }
    public class UserEntity : FullAuditedEntity<Guid>, ITagOwnerEntity<UserEntity, UserTag, Guid>
    {
        public string Name { get; set; }
        public List<UserTag> Tags { get; set; } = new();
    }

}

using Microsoft.EntityFrameworkCore;
using YS.Knife.EFCore;

namespace MixinDemo.Model
{

    [EFEntityStore]
    [ModelScopeDefaultValueSql(nameof(YS.Knife.Entity.CreationAuditedEntity<int>.CreationTime), typeof(DateTime), "current_timestamp")]

    public class DemoContext : DbContext
    {
        public DemoContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<LabelEntity> Labels { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            this.ApplyKnifeExtensions(modelBuilder);
        }
    }
}

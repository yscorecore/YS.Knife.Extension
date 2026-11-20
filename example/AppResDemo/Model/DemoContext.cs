using Microsoft.EntityFrameworkCore;
using YS.Knife.AppRes.Entity.EFCore;
using YS.Knife.EFCore;

namespace AppResDemo.Model
{
    [EFEntityStore(typeof(AppResourceEntity))]
    [EFEntityStore(typeof(AppFileResourceEntity))]
    [EFEntityStore(typeof(AppTextResourceEntity))]
    [ModelScopeDefaultValueSql(nameof(YS.Knife.Entity.BaseEntity<int>.CreateTime), typeof(DateTime), "current_timestamp")]
    public class DemoContext : DbContext
    {
        public DemoContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<AppResourceEntity> AppResources { get; set; }

        public DbSet<AppFileResourceEntity> AppFileResources { get; set; }

        public DbSet<AppTextResourceEntity> AppTextResources { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            this.ApplyKnifeExtensions(modelBuilder);
        }
    }
}

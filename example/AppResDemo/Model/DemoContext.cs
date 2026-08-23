using Microsoft.EntityFrameworkCore;
using YS.Knife.AppRes.Entity.EFCore;

namespace AppResDemo.Model
{
    [ModelScopeDefaultValueSql(nameof(YS.Knife.Entity.CreationAuditedEntity<int>.CreationTime), typeof(DateTime), "current_timestamp")]
    public class DemoContext : DbContext
    {
        public DbSet<AppResourceEntity> AppResources { get; set; }

        public DbSet<AppFileResourceEntity> AppFileResources { get; set; }

        public DbSet<AppTextResourceEntity> AppTextResources { get; set; }
    }
}

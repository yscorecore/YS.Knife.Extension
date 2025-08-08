using Microsoft.EntityFrameworkCore;
using YS.Knife.EFCore;
using YS.Knife.KeyValue.Impl.EFCore;

namespace KeyValueDemo
{
    [EFEntityStore(typeof(KeyValueEntity<Guid>))]
    public class KeyValueContext : DbContext
    {
        public KeyValueContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<KeyValueEntity<Guid>> KeyValues { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            this.ApplyKnifeExtensions(modelBuilder);
        }
    }


}

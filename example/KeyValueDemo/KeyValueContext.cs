using Microsoft.EntityFrameworkCore;
using YS.Knife.EfCore;
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
    }
}

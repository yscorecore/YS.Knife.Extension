using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using YS.Knife.EntityBase;

namespace YS.Knife.EfCore
{
    public class EFEntityStore<TEntity, TContext> : IEntityStore<TEntity>
              where TContext : DbContext
              where TEntity : class
    {
        public EFEntityStore(TContext context)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));
            this.context = context;
            this.set = context.Set<TEntity>();
        }



        private TContext context;

        private DbSet<TEntity> set;


        public IQueryable<TEntity> Current => this.set;

        public void Add(TEntity entity)
        {
            this.set.Add(entity);
        }

        public void Delete(TEntity entity)
        {
            this.set.Remove(entity);
        }

        public Task SaveChangesAsync()
        {
            return this.context.SaveChangesAsync();
        }
    }
}

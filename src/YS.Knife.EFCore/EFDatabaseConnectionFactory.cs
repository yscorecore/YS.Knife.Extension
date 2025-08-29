using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using YS.Knife.Data;

namespace YS.Knife.EFCore
{

    public class EFDatabaseConnectionFactory<T> : IDbConnectionFactory<T>
        where T : DbContext
    {
        private readonly Lazy<DbProviderFactory> providerFactory;
        private readonly Lazy<RelationalOptionsExtension> relationalOptionsExtension;
        public EFDatabaseConnectionFactory(T dbContext, DbContextOptions<T> options)
        {
            this.providerFactory = new Lazy<DbProviderFactory>(() =>
            {
                dbContext.Database.GetDbConnection();
                return DbProviderFactories.GetFactory(dbContext.Database.GetDbConnection());
            });
            this.relationalOptionsExtension = new Lazy<RelationalOptionsExtension>(() =>
            {
                return options.Extensions.OfType<RelationalOptionsExtension>().FirstOrDefault();
            });
        }

        public DbProviderFactory Factory => this.providerFactory.Value;

        public DbConnection CreateConnection()
        {
            var conn = providerFactory.Value.CreateConnection();
            conn.ConnectionString = relationalOptionsExtension.Value?.ConnectionString;
            return conn;
        }
    }

}

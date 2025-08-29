using System.ComponentModel;
using System.Data.Common;

namespace System.Data
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class DbProviderFactoryExtensions
    {
        public static DbConnection CreateConnection(this DbProviderFactory factory, string connectionString)
        {
            var connection = factory.CreateConnection();
            connection.ConnectionString = connectionString;
            return connection;
        }
    }
}

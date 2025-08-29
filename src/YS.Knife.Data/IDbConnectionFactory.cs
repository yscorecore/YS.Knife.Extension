using System.Data.Common;

namespace YS.Knife.Data
{
    public interface IDbConnectionFactory
    {
        DbConnection CreateConnection();

        DbProviderFactory Factory { get; }
    }
    public interface IDbConnectionFactory<T> : IDbConnectionFactory
    {

    }
}

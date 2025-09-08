using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using YS.Knife.Query;

namespace YS.Knife.DataSource
{
    public interface IDataSourceService
    {
        Task<List<string>> AllSources();
        Task<IPagedList> LoadData(string name, LimitQueryInfo queryInfo, CancellationToken cancellationToken = default);
        Task<PagedList<T>> LoadData<T>(string name, LimitQueryInfo queryInfo, CancellationToken cancellationToken = default);
        Task<long> Count(string name, string filter, CancellationToken cancellationToken = default);
        Task<object> FindBy(string name, string key, string value, CancellationToken cancellationToken = default);
        Task<T> FindBy<T>(string name, string key, string value, CancellationToken cancellationToken = default);
        Task<object> All(string name, QueryInfo queryInfo, CancellationToken cancellationToken = default);
        Task<List<T>> All<T>(string name, QueryInfo queryInfo, CancellationToken cancellationToken = default);
        Task<object> Agg(string name, LimitQueryInfo queryInfo, CancellationToken cancellationToken = default);
    }
}

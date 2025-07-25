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
        Task<IPagedList> LoadData(string name, LimitQueryInfo queryInfo);
        Task<long> Count(string name, string filter);
        Task<object> FindBy(string name, string key, string value);
        Task<object> All(string name, QueryInfo queryInfo);

        Task<object> Agg(string name, LimitQueryInfo queryInfo);
    }

}

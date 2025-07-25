using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using YS.Knife.Query;

namespace YS.Knife.DataSource.Core.Impl
{

    [Service]
    [AutoConstructor]
    public partial class DataSourceService : IDataSourceService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IOptions<DataSourceOptions> options;

        public Task<IPagedList> LoadData(string name, LimitQueryInfo queryInfo)
        {
            return GetDataSourceFunc(name)(serviceProvider, queryInfo);
        }
        private Func<IServiceProvider, LimitQueryInfo, Task<IPagedList>> GetDataSourceFunc(string name)
        {
            if (options.Value.DataSources.TryGetValue(name, out var sourceInfo))
            {
                return sourceInfo;
            }
            throw new Exception($"can not find data source '{name}'");
        }

        public Task<List<string>> AllSources()
        {
            return Task.FromResult(options.Value.DataSources.Keys.ToList());
        }

        public async Task<long> Count(string name, string filter)
        {
            var res = await GetDataSourceFunc(name)(serviceProvider, new LimitQueryInfo
            {
                Filter = filter
            });
            return res.TotalCount;
        }

        public async Task<object> FindBy(string name, string key, string value)
        {
            dynamic res = await GetDataSourceFunc(name)(serviceProvider, new LimitQueryInfo
            {
                Limit = 1,
                Filter = $"{key}='{value}'"
            });
            return res.Items.Count > 0 ? res.Items[0] : default;
        }

        public async Task<object> All(string name, QueryInfo queryInfo)
        {
            dynamic res = await GetDataSourceFunc(name)(serviceProvider, new LimitQueryInfo
            {
                Limit = 10000,//TODO 从配置中加载
                Filter = queryInfo.Filter,
                Select = queryInfo.Select,
                OrderBy = queryInfo.OrderBy,
            });
            return res.Items;
        }

        public async Task<object> Agg(string name, LimitQueryInfo queryInfo)
        {
            dynamic res = await GetDataSourceFunc(name)(serviceProvider, new LimitQueryInfo
            {
                Agg = queryInfo.Agg,
                Filter = queryInfo.Filter,
            });
            return res.Aggs;
        }
    }

}

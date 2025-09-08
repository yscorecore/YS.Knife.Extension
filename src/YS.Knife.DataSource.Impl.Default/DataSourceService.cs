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

        public Task<IPagedList> LoadData(string name, LimitQueryInfo queryInfo, CancellationToken cancellationToken = default)
        {
            return GetDataSourceFunc(name)(serviceProvider, queryInfo, cancellationToken);
        }
        private Func<IServiceProvider, LimitQueryInfo, CancellationToken, Task<IPagedList>> GetDataSourceFunc(string name)
        {
            if (AssemblyDataSourceEntryFinder.Instance.All.TryGetValue(name, out var sourceInfo))
            {
                return (IServiceProvider sp, LimitQueryInfo queryInfo, CancellationToken token) =>
                {
                    var dataSourceType = sourceInfo.EntityType;
                    var serviceInstance = sp.GetRequiredService(sourceInfo.ServiceType);
                    return InvokeMethod(sourceInfo, serviceInstance, queryInfo, token);
                };
            }
            throw new Exception($"can not find data source '{name}'");
        }

        private Task<IPagedList> InvokeMethod(DataSourceEntry entry, object instance, LimitQueryInfo limit, CancellationToken token)
        {

            ITaskInvoker invoker = Activator.CreateInstance(typeof(TaskInvoker<>).MakeGenericType(entry.EntityType)) as ITaskInvoker;
            if (entry.IsValueTask)
            {
                return invoker.InvokeValueTaskMethod(entry, instance, limit, token);
            }
            else
            {
                return invoker.InvokeTaskMethod(entry, instance, limit, token);
            }
        }
        interface ITaskInvoker
        {
            Task<IPagedList> InvokeValueTaskMethod(DataSourceEntry method, object service, LimitQueryInfo limit, CancellationToken token);
            Task<IPagedList> InvokeTaskMethod(DataSourceEntry method, object service, LimitQueryInfo limit, CancellationToken token);
        }
        class TaskInvoker<T> : ITaskInvoker
        {
            public async Task<IPagedList> InvokeTaskMethod(DataSourceEntry method, object service, LimitQueryInfo limit, CancellationToken token)
            {
                var task = (Task<PagedList<T>>)(method.Method.Invoke(service, CombinArguments(method, limit, token)));
                var res = await task;
                return res;
            }

            public async Task<IPagedList> InvokeValueTaskMethod(DataSourceEntry method, object service, LimitQueryInfo limit, CancellationToken token)
            {
                var task = (ValueTask<PagedList<T>>)(method.Method.Invoke(service, CombinArguments(method, limit, token)));
                var res = await task;
                return res;
            }
            private object[] CombinArguments(DataSourceEntry entry, LimitQueryInfo limit, CancellationToken token)
            {
                return entry.HasCancellationToken ?
                    (entry.Arguments ?? Array.Empty<object>()).Concat(new object[] { limit, token }).ToArray()
                    : (entry.Arguments ?? Array.Empty<object>()).Concat(new object[] { limit }).ToArray();

            }
        }



        public Task<List<string>> AllSources()
        {
            return Task.FromResult(AssemblyDataSourceEntryFinder.Instance.All.Keys.ToList());
        }

        public async Task<long> Count(string name, string filter, CancellationToken cancellationToken = default)
        {
            var res = await GetDataSourceFunc(name)(serviceProvider, new LimitQueryInfo
            {
                Filter = filter,
            }, cancellationToken);
            return res.TotalCount;
        }

        public async Task<object> FindBy(string name, string key, string value, CancellationToken cancellationToken = default)
        {
            dynamic res = await GetDataSourceFunc(name)(serviceProvider, new LimitQueryInfo
            {
                Limit = 1,
                Filter = $"{key}='{value}'"
            }, cancellationToken);
            return res.Items.Count > 0 ? res.Items[0] : default;
        }

        public async Task<object> All(string name, QueryInfo queryInfo, CancellationToken cancellationToken = default)
        {
            dynamic res = await GetDataSourceFunc(name)(serviceProvider, new LimitQueryInfo
            {
                Limit = 10000,//TODO 从配置中加载
                Filter = queryInfo.Filter,
                Select = queryInfo.Select,
                OrderBy = queryInfo.OrderBy,
            }, cancellationToken);
            return res.Items;
        }

        public async Task<object> Agg(string name, LimitQueryInfo queryInfo, CancellationToken cancellationToken = default)
        {
            dynamic res = await GetDataSourceFunc(name)(serviceProvider, new LimitQueryInfo
            {
                Agg = queryInfo.Agg,
                Filter = queryInfo.Filter,
            }, cancellationToken);
            return res.Aggs;
        }

        public async Task<PagedList<T>> LoadData<T>(string name, LimitQueryInfo queryInfo, CancellationToken cancellationToken = default)
        {
            var res = await LoadData(name, queryInfo, cancellationToken);
            return (PagedList<T>)res;
        }

        public async Task<T> FindBy<T>(string name, string key, string value, CancellationToken cancellationToken = default)
        {
            var res = await FindBy(name, key, value, cancellationToken);
            return (T)res;
        }

        public async Task<List<T>> All<T>(string name, QueryInfo queryInfo, CancellationToken cancellationToken = default)
        {
            var res = await All(name, queryInfo, cancellationToken);
            return (List<T>)res;
        }
    }

}

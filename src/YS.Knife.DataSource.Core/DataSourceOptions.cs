using YS.Knife.Query;

namespace YS.Knife.DataSource
{
    public class DataSourceOptions
    {
        public IDictionary<string, Func<IServiceProvider, LimitQueryInfo, Task<IPagedList>>> DataSources { get; } = new Dictionary<string, Func<IServiceProvider, LimitQueryInfo, Task<IPagedList>>>();

    }
    public static class DataSourceOptionsExtensions
    {
        public static DataSourceOptions Add<T>(this DataSourceOptions options, string name, Func<IServiceProvider, LimitQueryInfo, Task<PagedList<T>>> dataFactory)
        {
            _ = dataFactory ?? throw new ArgumentNullException(nameof(dataFactory));
            options.DataSources[name] = async (sp, limit) =>
                {
                    var data = await dataFactory(sp, limit);
                    return data;
                };
            return options;
        }
        public static DataSourceOptions Add(this DataSourceOptions options, string name, Func<IServiceProvider, LimitQueryInfo, Task<IPagedList>> dataFactory)
        {
            _ = dataFactory ?? throw new ArgumentNullException(nameof(dataFactory));
            options.DataSources[name] = dataFactory;
            return options;
        }
        public static DataSourceOptions Clear(this DataSourceOptions options)
        {
            options.DataSources.Clear();
            return options;
        }
    }
}

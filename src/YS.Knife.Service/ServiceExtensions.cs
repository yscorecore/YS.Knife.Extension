using YS.Knife.Query;

namespace YS.Knife.Service
{
    public static class ServiceExtensions
    {
        public static async Task<TKey> Create<TCreateDto, TKey>(this ICreateApi<TCreateDto, TKey> api, TCreateDto Dto, CancellationToken token = default)
             where TCreateDto : class
        {
            var res = await api.Create(new TCreateDto[] { Dto }, token);
            return res.Single();
        }
        public static Task Update<TUpdateDto, TKey>(this IUpdateApi<TUpdateDto, TKey> api, TKey key, TUpdateDto Dto, CancellationToken token = default)
             where TUpdateDto : class
        {
            return api.Update(new TKey[] { key }, Dto, token);
        }
        public static Task Delete<TKey>(this IDeleteApi<TKey> api, TKey key, CancellationToken token = default)
        {
            return api.Delete(new TKey[] { key }, token);
        }

        public static async Task<List<TResDto>> QueryList<TResDto>(this IQueryPageApi<TResDto> api, QueryInfo? req, int maxSize = 10000, CancellationToken cancellationToken = default)
        {
            var res = await api.QueryPagedList(new LimitQueryInfo
            {
                Filter = req?.Filter,
                Select = req?.Select,
                OrderBy = req?.OrderBy,
                Limit = maxSize,
            }, cancellationToken);
            return res.Items;
        }
        public static async Task<TResDto?> QueryOne<TResDto, TKey>(this IQueryPageApi<TResDto> api, TKey key, CancellationToken cancellationToken = default)
            where TResDto : IIdDto<TKey>
        {
            var res = await api.QueryPagedList(new LimitQueryInfo
            {
                Filter = $"{nameof(IIdDto<TKey>.Id)} =='{key}'",
                Limit = 2,
            }, cancellationToken);
            return res.Items.SingleOrDefault();
        }
        public static async Task<long> Count<TResDto>(this IQueryPageApi<TResDto> api, string? filter = default, CancellationToken cancellationToken = default)
        {
            var res = await api.QueryPagedList(new LimitQueryInfo
            {
                Filter = filter,
                Limit = 0,
            }, cancellationToken);
            return res.TotalCount;
        }
    }

   
}

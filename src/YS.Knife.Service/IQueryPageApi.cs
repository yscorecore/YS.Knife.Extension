using YS.Knife.Query;

namespace YS.Knife.Service
{
    public interface IQueryPageApi<TResDto>
    {
        Task<PagedList<TResDto>> QueryPagedList(LimitQueryInfo req, CancellationToken cancellationToken = default);
    }

   
}

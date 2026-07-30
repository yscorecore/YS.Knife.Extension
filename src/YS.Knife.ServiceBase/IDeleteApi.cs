using YS.Knife.Operations;

namespace YS.Knife.Service
{
    public interface IDeleteApi<TKey>
    {
        [Operation("delete", "删除{name}")]
        Task Delete(TKey[] ids, CancellationToken token = default);
    }


}

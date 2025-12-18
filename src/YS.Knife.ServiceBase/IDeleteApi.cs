namespace YS.Knife.Service
{
    public interface IDeleteApi<TKey>
    {
        Task Delete(TKey[] ids, CancellationToken token = default);
    }


}

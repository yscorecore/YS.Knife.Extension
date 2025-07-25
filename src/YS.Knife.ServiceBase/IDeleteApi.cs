namespace YS.Knife.Service
{
    public interface IDeleteApi<TKey>
    {
        Task Delete(TKey[] keys, CancellationToken token = default);
    }


}

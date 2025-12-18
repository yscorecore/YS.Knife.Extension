namespace YS.Knife.Service
{
    public interface IUpdateApi<TUpdateDto, TKey>
         where TUpdateDto : class
    {
        Task Update(TKey[] ids, TUpdateDto dto, CancellationToken token = default);
    }
}

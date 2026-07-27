namespace YS.Knife.Service
{
    public interface IUpdateApi<TUpdateDto, TKey>
         where TUpdateDto : class, IIdDto<TKey>
    {
        
        Task Update(TUpdateDto[] dtos, CancellationToken token = default);
    }
}

namespace YS.Knife.Service
{
    public interface ICreateApi<TCreateDto, TKey>
        where TCreateDto : class
    {
        Task<TKey[]> Create(TCreateDto[] dtos, CancellationToken token = default);
    }


}

using YS.Knife.Operations;

namespace YS.Knife.Service
{
    public interface ICreateApi<TCreateDto, TKey>
        where TCreateDto : class
    {
        [Operation("create", "创建{name}")]
        Task<TKey[]> Create(TCreateDto[] dtos, CancellationToken token = default);
    }


}

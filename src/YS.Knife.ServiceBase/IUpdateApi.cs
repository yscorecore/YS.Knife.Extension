using YS.Knife.Operations;

namespace YS.Knife.Service
{
    public interface IUpdateApi<TUpdateDto, TKey>
         where TUpdateDto : class, IIdDto<TKey>
    {
        [Operation("update", "更新{name}")]
        Task Update(TUpdateDto[] dtos, CancellationToken token = default);
    }
}

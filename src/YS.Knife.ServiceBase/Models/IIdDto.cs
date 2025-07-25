namespace YS.Knife.Service
{
    public interface IIdDto<TKey>
    {
        TKey Id { get; }
    }
}

namespace YS.Knife.Service
{
    public record IdDto<TKey> : IIdDto<TKey>
    {
        public TKey Id { get; set; }
    }
}

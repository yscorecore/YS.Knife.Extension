namespace YS.Knife.EntityBase
{
    public interface IEntity<TKey>
        where TKey : notnull
    {
        public TKey Id { get; }
    }
}

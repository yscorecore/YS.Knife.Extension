namespace YS.Knife.Entity
{
    public interface IEntity<TKey>
        where TKey : notnull
    {
        public TKey Id { get; }
    }
}

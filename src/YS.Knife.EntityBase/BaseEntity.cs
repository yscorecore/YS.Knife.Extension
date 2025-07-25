namespace YS.Knife.Entity
{
    public class BaseEntity<TKey> : IEntity<TKey>
    {
        public virtual TKey Id { get; set; }
        public virtual DateTime CreateTime { get; set; }
        public virtual string CreateUser { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YS.Knife.Entity
{
    public class BaseEntity<TKey> : IEntity<TKey>
        where TKey : notnull
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual TKey Id { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public virtual DateTime CreateTime { get; set; }
        [StringLength(64)]
        public virtual string CreateUser { get; set; }
    }

    public interface ITagOwnerEntity<TOwner, TTag, TKey>
        where TTag : Tag<TOwner, TKey>
        where TOwner : ITagOwnerEntity<TOwner, TTag, TKey>
    {
        public List<TTag> Tags { get; set; }
    }

    public abstract class Tag<TOwner, TKey> : BaseEntity<TKey>
        where TKey : notnull
    {
        public TOwner Owner { get; set; }

        [StringLength(32)]
        public string Group { get; set; }
        [StringLength(64)]
        [Required]
        public string Value { get; set; }
    }
}

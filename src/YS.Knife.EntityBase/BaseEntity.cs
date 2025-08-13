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
}

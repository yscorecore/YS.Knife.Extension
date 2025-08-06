using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using YS.Knife.Entity;

namespace YS.Knife.KeyValue.Impl.EFCore
{
    [Index(nameof(Key), IsUnique = true)]
    public class KeyValueEntity<T> : BaseEntity<T>
    {
        [Required]
        [StringLength(64)]
        public string Key { get; set; }
        public string Value { get; set; }
    }
}

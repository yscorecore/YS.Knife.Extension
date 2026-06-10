using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using YS.Knife.Entity;

namespace YS.Knife.EnumCode.Loader.Entity.EFCore
{
    [Index(nameof(Kind), nameof(Key), IsUnique = true)]
    public class EnumCodeEntity<T> : BaseEntity<T>, ISortableEntity, ISoftDeleteEntity
        where T : notnull
    {
        [Required]
        [StringLength(64)]
        public string Kind { get; set; } = null!;
        public int Key { get; set; }
        [StringLength(32)]
        public string Name { get; set; } = null!;
        [StringLength(256)]
        public string? Description { get; set; }
        [StringLength(64)]
        public string? Group { get; set; }
        public int Order { get; set; }

        public bool IsDeleted { get; set; }
    }
}

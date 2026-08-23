using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using YS.Knife.Entity;

namespace YS.Knife.AppRes.Entity.EFCore
{
    [Index(nameof(Group))]
    [Index(nameof(Group), nameof(Code), IsUnique = true)]
    public abstract class BaseResourceEntity<T> : BaseEntity<T>, IExtensibleEntity
        where T : notnull
    {
        [StringLength(64)]
        [Required]
        public string Group { get; set; } = null!;

        [StringLength(64)]
        [Required]
        public string Name { get; set; } = null!;
        [StringLength(64)]
        public string Code { get; set; } = null!;

        [StringLength(256)]
        public string? Description { get; set; }



        [JsonContent]
        public Dictionary<string, object>? Properties { get; set; }
    }

    public abstract class BaseResourceEntity : BaseResourceEntity<Guid> { }
}

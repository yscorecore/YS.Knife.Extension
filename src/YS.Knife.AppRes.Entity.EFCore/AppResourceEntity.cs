using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using YS.Knife.Entity;

namespace YS.Knife.AppRes.Entity.EFCore
{
    [Index(nameof(Group))]
    [Index(nameof(Group), nameof(Code), IsUnique = true)]
    public abstract class AppResourceEntity<T> : BaseEntity<T>, IExtensibleEntity, ISortableEntity
        where T : notnull
    {
        [Comment("分组")]
        [StringLength(64)]
        [Required]
        public string Group { get; set; } = null!;
        [Comment("名称")]
        [StringLength(64)]
        [Required]
        public string Name { get; set; } = null!;
        [Comment("编码")]
        [StringLength(64)]
        public string Code { get; set; } = null!;
        [Comment("描述")]
        [StringLength(256)]
        public string? Description { get; set; }

        [Comment("拓展属性")]
        [JsonContent]
        public Dictionary<string, object>? Properties { get; set; }
        [Comment("排序")]
        public int Order { get; set; }
        [Comment("是否固定资源")]
        public bool Fixed { get; set; }
    }

    public abstract class AppResourceEntity : AppResourceEntity<Guid> { }
}

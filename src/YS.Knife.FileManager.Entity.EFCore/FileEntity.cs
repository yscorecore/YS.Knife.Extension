using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using YS.Knife.Entity;

namespace YS.Knife.FileManager.Entity.EFCore
{

    [Index(nameof(Owner))]
    [Index(nameof(Owner), nameof(ParentId), nameof(Name), IsUnique = true)]
    public class FileEntity<T> : BaseEntity<T>, ISoftDeleteEntity
        where T : struct
    {
        [StringLength(128)]
        [Required(AllowEmptyStrings = false)]
        public string Name { get; set; }
        [StringLength(8)]
        public string Extension { get; set; }
        [StringLength(128)]
        public string Key { get; set; }
        [StringLength(256)]
        public string Url { get; set; }
        public long Size { get; set; }
        [StringLength(64)]
        [Required]
        public string Owner { get; set; }
        public bool IsDeleted { get; set; }

        public T? ParentId { get; set; }
        public FileEntity<T> Parent { get; set; }
        public bool IsFolder { get; set; }
    }
}

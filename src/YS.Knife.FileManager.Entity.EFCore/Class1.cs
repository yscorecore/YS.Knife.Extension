using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using YS.Knife.Entity;

namespace YS.Knife.FileManager.Entity.EFCore
{
    [Index(nameof(Folder))]
    [Index(nameof(Key), IsUnique = true)]
    [Index(nameof(Name))]
    public class FileEntity<T> : BaseEntity<T>
    {
        [StringLength(256)]
        [Required(AllowEmptyStrings = false)]
        public string Key { get; set; }
        [StringLength(128)]
        [Required(AllowEmptyStrings = false)]
        public string Name { get; set; }

        [StringLength(512)]
        public string Url { get; set; }
        [StringLength(256)]
        public string Folder { get; set; }
        public long Size { get; set; }

    }

    public class FileChangeHistory<T> : BaseEntity<T>
    {

    }
}

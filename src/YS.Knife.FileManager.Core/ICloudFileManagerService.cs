using System.ComponentModel.DataAnnotations;
using YS.Knife.Service;

namespace YS.Knife.FileManager
{
    public interface ICloudFileManagerService : IDeleteApi<Guid>, ICreateApi<CreateFileDto<Guid>, Guid>
    {
        Task Rename(RenameFileDto<Guid> renameFileDto, CancellationToken cancellationToken = default);
    }
    public class CreateFileDto<T>
        where T : struct
    {
        [Required]
        [StringLength(128, MinimumLength = 1)]
        [RegularExpression(@"^\\w+$", ErrorMessage = "文件名称不符合要求")]
        public string Name { get; set; }
        public string Key { get; set; }
        public string Url { get; set; }
        public long Size { get; set; }
        public T? ParentId { get; set; }
        public bool IsFolder { get; set; }
    }

    public class RenameFileDto<T>
    {
        public T Id { get; set; }
        [Required]
        [StringLength(128, MinimumLength = 1)]
        [RegularExpression(@"^\\w+$", ErrorMessage = "文件名称不符合要求")]
        public string Name { get; set; }
    }
    public record FileDto<T> : BaseDto<T>
        where T : struct
    {
        public string Name { get; set; }
        public string Key { get; set; }
        public string Url { get; set; }
        public long Size { get; set; }
        public string Owner { get; set; }
        public T? ParentId { get; set; }
        public bool IsFolder { get; set; }
    }
}

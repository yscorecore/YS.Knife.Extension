using YS.Knife.DataSource;
using YS.Knife.Query;
using YS.Knife.Service;

namespace YS.Knife.FileManager
{
    public interface IFileManagerService
    {

    }
    public interface IFileService//: IQueryPageApi<FileDto>
    {
        [DataSource("files")]
        Task<PagedList<FileDto>> QueryPagedList(LimitQueryInfo req, CancellationToken cancellationToken = default);
    }
    public record FileDto
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string Folder { get; set; }
        public long Size { get; set; }
    }
}

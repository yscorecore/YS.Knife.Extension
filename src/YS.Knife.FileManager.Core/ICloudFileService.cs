using YS.Knife.DataSource;
using YS.Knife.Query;

namespace YS.Knife.FileManager
{
    public interface ICloudFileService
    {
        [DataSource("cloud-files")]
        Task<PagedList<FileDto<Guid>>> QueryPagedList(LimitQueryInfo req, CancellationToken cancellationToken = default);
    }
}

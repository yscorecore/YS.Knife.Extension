
namespace YS.Knife.Documents.Preview.Impl.OfficeApps
{
    public class DocumentOnlinePreviewService : IDocumentOnlinePreviewService
    {
        public Task<string> GetPreviewUrl(string fileUrl, string title, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetPreviewUrl(string fileUrl, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}

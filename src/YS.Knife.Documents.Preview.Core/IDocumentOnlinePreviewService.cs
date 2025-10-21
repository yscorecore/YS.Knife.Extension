using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YS.Knife.Documents.Preview
{
    public interface IDocumentOnlinePreviewService
    {
        Task<string> GetPreviewUrl(string fileUrl, string title, CancellationToken cancellationToken = default);
    }
}

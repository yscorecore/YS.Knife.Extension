using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YS.Knife.Documents.Preview
{
    public interface IDocumentOnlinePreviewService
    {
        Task<string> GetPreviewUrl(string fileUrl, CancellationToken cancellationToken = default);
    }
    public interface IDocumentOnlineConvertService
    {
        Task<string> GetConvertUrl(string fileUrl, string targetFromat, CancellationToken cancellationToken = default);
    }
    public interface IDocumentOnlinePrintService
    {
        Task<string> GetPrintUrl(string fileUrl, CancellationToken cancellationToken = default);
    }
}

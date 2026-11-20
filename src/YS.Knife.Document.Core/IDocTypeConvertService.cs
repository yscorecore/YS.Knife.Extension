using System.Text.Json;

namespace YS.Knife.Document
{
    public interface IDocTypeConvertService
    {
        StreamBody Convert(StreamBody input, string converter, object option, CancellationToken token = default);
    }
}

using System.Text.Json;

namespace YS.Knife.Document
{
    public interface IDocTypeConvertHandler
    {
        public static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        string Name { get; }
        StreamBody Convert(StreamBody input, object options, CancellationToken token = default);
    }
}

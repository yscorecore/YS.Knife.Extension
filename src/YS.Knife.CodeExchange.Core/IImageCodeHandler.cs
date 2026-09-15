using System.Text.Json;

namespace YS.Knife.CodeExchange
{
    public interface IImageCodeHandler
    {
        public static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };

        string Name { get; }
        TimeSpan ExpiresIn { get; }
        ImageCodeDataKind DataKind { get; }
        Task<(string Sence, Stream ImageStream)> GeneratorCode(object args, CancellationToken cancellationToken);
        Task<ArgDataPair> ProcessData(object args, object data, CancellationToken cancellationToken);
        public record ArgDataPair(object Args, object Data);
    }

}

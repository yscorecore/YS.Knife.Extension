
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace YS.Knife.Document.Impl.Default
{
    [Service]
    [AutoConstructor]
    [Logger]
    public partial class DocTypeConvertService : IDocTypeConvertService
    {
        private readonly IEnumerable<IDocTypeConvertHandler> _handlers;
        public StreamBody Convert(StreamBody input, string converter, object option, CancellationToken token = default)
        {
            var handler = _handlers.Where(p => string.Equals(p.Name, converter, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (handler != null)
            {
                logger.LogInformation("Convert document type {converter} from {input.ContentType} to {outputContentType}", converter, input.ContentType, handler.Name);
                var res = handler.Convert(input, option, token);
                logger.LogInformation("Convert document type {converter} from {input.ContentType} to {outputContentType} completed", converter, input.ContentType, handler.Name);
                return res;
            }
            else
            {
                throw new NotSupportedException($"Document converter {converter} not supported");
            }
        }
    }
}

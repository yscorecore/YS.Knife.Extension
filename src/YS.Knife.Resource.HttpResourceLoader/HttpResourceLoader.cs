using Microsoft.Extensions.Logging;

namespace YS.Knife.Resource
{
    [AutoConstructor]
    [Service(Lifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton)]
    public partial class HttpResourceLoader : IResourceLoader
    {
        private readonly HttpClient httpClient;
        private readonly HttpResourceOptions options;
        private readonly ILogger<HttpResourceLoader> logger;

        public virtual int Priority => 2000;
        public virtual bool CanLoad(string templateUri)
        {
            _ = templateUri ?? throw new ArgumentNullException(nameof(templateUri));
            return Uri.IsWellFormedUriString(templateUri, UriKind.Absolute);
        }

        public virtual Task<Stream> LoadTemplate(string templateUri)
        {
            logger.LogInformation("Loading template from the url {url}", templateUri);
            return httpClient.DownloadStreamWithCache(templateUri, options.CacheFolder, options.RefreshCache);
        }
    }
}

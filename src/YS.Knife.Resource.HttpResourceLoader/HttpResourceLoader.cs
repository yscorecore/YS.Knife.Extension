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

        public virtual async Task<Stream> LoadTemplate(string templateUri)
        {
            var cacheFile = await Download(templateUri, options.CacheFolder);
            return File.OpenRead(cacheFile);
        }

        public async Task<string> Download(string uri, string path)
        {
            var cacheFile = await httpClient.DownloadWithCache(uri, path, options.RefreshCache);
            logger.LogInformation("Template cache file ready for the url {url}", uri);
            return cacheFile;
        }
    }
}

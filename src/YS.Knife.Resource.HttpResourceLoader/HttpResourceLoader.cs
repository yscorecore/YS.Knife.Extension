using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace YS.Knife.Resource
{
    [AutoConstructor]
    [Service(Lifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton)]
    public partial class HttpResourceLoader : IResourceLoader
    {
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
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            var cacheFileName = Path.Combine(path, BuildCacheFileName(uri));
            await DownloadFileWithCache(uri, cacheFileName);
            return cacheFileName;
        }

        private string BuildCacheFileName(string uri)
        {
            var targetSpan = new char[uri.Length];
            uri.AsSpan().CopyTo(targetSpan);
            var invalidFileNameChars = Path.GetInvalidFileNameChars();
            for (int i = 0; i < targetSpan.Length; i++)
            {
                if (invalidFileNameChars.Contains(targetSpan[i]))
                {
                    targetSpan[i] = '_';
                }
            }
            return new string(targetSpan.ToArray());
        }

        private async Task DownloadFileWithCache(string uri, string filePath)
        {
            if (File.Exists(filePath))
            {
                DateTime lastModified = File.GetLastWriteTime(filePath);

                var client = new HttpClient();
                client.DefaultRequestHeaders.IfModifiedSince = lastModified;

                var response = await client.GetAsync(uri);

                if (response.StatusCode == HttpStatusCode.NotModified)
                {
                    logger.LogTrace("Template cache file not modified for the url {url}", uri);
                }
                else if (response.IsSuccessStatusCode)
                {
                    using var stream = await response.Content.ReadAsStreamAsync();
                    using var fileStream = File.OpenWrite(filePath);
                    stream.CopyTo(fileStream);
                    logger.LogInformation("Template cache file updated for the url {url}", uri);
                }
            }
            else
            {
                var client = new HttpClient();
                var response = await client.GetAsync(uri);
                if (response.IsSuccessStatusCode)
                {
                    using var stream = await response.Content.ReadAsStreamAsync();
                    using var fileStream = File.OpenWrite(filePath);
                    stream.CopyTo(fileStream);
                    logger.LogInformation("Template cache file download for the url {url}", uri);
                }
            }
        }


    }
}

using Microsoft.EntityFrameworkCore;
using YS.Knife.AppRes.Entity.EFCore;
using YS.Knife.Entity;

namespace YS.Knife.Resource.AppFileResLoader
{
    [Service]
    [AutoConstructor]
    public partial class AppResourceLoader : IResourceLoader
    {
        public virtual int Priority => 11000;
        private readonly IEntityStore<AppResourceEntity> entityStore;
        private readonly AppResourceOptions options;
        private readonly HttpClient httpClient;

        public virtual bool CanLoad(string templateUri)
        {
            return Guid.TryParse(templateUri, out _);
        }

        public async Task<Stream> LoadTemplate(string templateUri)
        {
            var id = Guid.Parse(templateUri);
            var appresourceEntity = await entityStore.Current.Where(p => p.Id == id).FindOrThrowAsync();
            return await LoadAppSourceEntity(appresourceEntity, httpClient, options, default);
        }
        internal static async Task<Stream> LoadAppSourceEntity(AppResourceEntity appresourceEntity, HttpClient httpClient, AppResourceOptions options, CancellationToken cancellationToken = default)
        {
            if (appresourceEntity is AppFileResourceEntity appFile)
            {
                var url = ReplaceUrl(appFile.FileUrl, options.UrlMappings);
                return await httpClient.DownloadStreamWithCache(url, options.CacheFolder, options.RefreshCache, cancellationToken);
            }
            else if (appresourceEntity is AppTextResourceEntity appText)
            {
                var ms = new MemoryStream();
                await using StringWriter sw = new StringWriter();
                await sw.WriteLineAsync(appText.Content ?? string.Empty);
                ms.Seek(0, SeekOrigin.Begin);
                return ms;
            }
            else
            {
                throw new Exception($"Can not know appResource type '{appresourceEntity.GetType()}.'");
            }
        }

        internal static string ReplaceUrl(string url, Dictionary<string, string>? urlMappings)
        {
            if (urlMappings == null || urlMappings.Count == 0 || string.IsNullOrEmpty(url))
            {
                return url;
            }
            foreach (var mapping in urlMappings)
            {
                if (url.StartsWith(mapping.Key, StringComparison.OrdinalIgnoreCase))
                {
                    return mapping.Value + url.Substring(mapping.Key.Length);
                }
            }
            return url;
        }

    }
}

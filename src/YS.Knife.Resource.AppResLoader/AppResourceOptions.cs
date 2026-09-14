using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace YS.Knife.Resource.AppFileResLoader
{
    [Options]
    public class AppResourceOptions
    {
        [Required]
        public string CacheFolder { get; set; } = null!;

        /// <summary>
        /// Whether to revalidate an existing cache file against the server (via ETag and Last-Modified).
        /// When false, an existing cache file is used directly without any request.
        /// </summary>
        public bool RefreshCache { get; set; } = true;

        /// <summary>
        /// URL prefix mappings for download address replacement.
        /// Key is the prefix to match in the original URL, value is the replacement prefix.
        /// For example: { "https://www.abc.com/": "http://10.0.12.12/" }
        /// When a resource URL starts with a key, it will be replaced with the corresponding value before downloading.
        /// </summary>
        public Dictionary<string, string> UrlMappings { get; set; } = new();

    }
    [OptionsPostHandler]
    public class ExportOptionsPostHandler : IPostConfigureOptions<AppResourceOptions>
    {
        public void PostConfigure(string name, AppResourceOptions options)
        {
            if (string.IsNullOrEmpty(options.CacheFolder))
            {
                options.CacheFolder = Path.GetTempPath();
            }
        }
    }
}

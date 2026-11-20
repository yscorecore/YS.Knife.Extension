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
        /// For example: [{ "pattern": "https://www.abc.com/", "replacement": "http://10.0.12.12/" }]
        /// When a resource URL starts with a pattern, it will be replaced with the corresponding replacement before downloading.
        /// </summary>
        public List<UrlMapping> UrlMappings { get; set; } = new();

    }

    public class UrlMapping
    {
        public string Pattern { get; set; } = string.Empty;

        public string Replacement { get; set; } = string.Empty;
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

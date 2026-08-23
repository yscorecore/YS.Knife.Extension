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

using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace YS.Knife.Resource
{
    [Options]
    public class HttpResourceOptions
    {
        [Required]
        public string CacheFolder { get; set; }


        public bool CheckRemoteLastModiedTime { get; set; } = true;


    }

    [OptionsPostHandler]
    public class ExportOptionsPostHandler : IPostConfigureOptions<HttpResourceOptions>
    {
        public void PostConfigure(string name, HttpResourceOptions options)
        {
            if (string.IsNullOrEmpty(options.CacheFolder))
            {
                options.CacheFolder = Path.GetTempPath();
            }
        }
    }
}

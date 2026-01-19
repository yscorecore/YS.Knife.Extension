using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Microsoft.Extensions.Options;

namespace YS.Knife.Import.Impl.Base
{
    [Options]
    public class ImportOptions
    {
        public string DataFolder { get; set; } = "import_data";
        //过期时长，最大长度, 最长的导入时间
        [Range(1, 60 * 60 * 24)]
        public int ExpiredIn { get; set; } = 7200;
    }
    [OptionsPostHandler]
    public class ExportOptionsPostHandler : IPostConfigureOptions<ImportOptions>
    {
        public void PostConfigure(string name, ImportOptions options)
        {
            if (string.IsNullOrEmpty(options.DataFolder))
            {
                options.DataFolder = Path.GetTempPath();
            }
        }
    }
}

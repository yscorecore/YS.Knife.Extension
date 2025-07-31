using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace YS.Knife.Export.Impl
{
    [Options]
    public class ExportOptions
    {
        public string DataFolder { get; set; }
        //过期时长，最大长度, 最长的导入时间
        [Range(1, 60 * 60 * 24)]
        public int ExpiredIn { get; set; } = 600;
    }

    [OptionsPostHandler]
    public class ExportOptionsPostHandler : IPostConfigureOptions<ExportOptions>
    {
        public void PostConfigure(string name, ExportOptions options)
        {
            if (string.IsNullOrEmpty(options.DataFolder))
            {
                options.DataFolder = Path.GetTempPath();
            }
        }
    }
}

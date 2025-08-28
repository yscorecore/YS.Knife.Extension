
using System.ComponentModel.DataAnnotations;

namespace YS.Knife.Tts.Impl.Aliyun
{
    [Options]
    public class AliyunTtsOptions
    {
        [Required(AllowEmptyStrings = false)]
        public string BaseUrl { get; set; } = "http://nls-gateway.cn-shanghai.aliyuncs.com";

        public string TokenUrl { get; set; } = "http://nls-meta.cn-shanghai.aliyuncs.com/";
        [Required(AllowEmptyStrings = false)]
        public string RegionId { get; set; } = "cn-shanghai";

        [Required(AllowEmptyStrings = false)]
        public string AccessKeyId { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string AccessKeySecret { get; set; }
        [Required(AllowEmptyStrings = false)]
        public string AppKey { get; set; }
    }
}

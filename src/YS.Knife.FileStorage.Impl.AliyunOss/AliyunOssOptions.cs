using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YS.Knife.FileStorage.AliyunOss
{
    [Options("Oss")]
    public class AliyunOssOptions
    {
        [Required(AllowEmptyStrings = false)]
        public string Endpoint { get; set; }
        [Required(AllowEmptyStrings = false)]
        public string BucketName { get; set; }
        [Required(AllowEmptyStrings = false)]
        public string AccessKeyId { get; set; }
        [Required(AllowEmptyStrings = false)]
        public string AccessKeySecret { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string PublicPoint { get; set; }
    }
}

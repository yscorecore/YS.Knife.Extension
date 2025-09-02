using System.ComponentModel.DataAnnotations;

namespace YS.Knife.FileStorage.Minio
{
    [Options]
    public class MinioOptions
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

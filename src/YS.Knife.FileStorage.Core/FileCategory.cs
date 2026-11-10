using System.ComponentModel.DataAnnotations;

namespace YS.Knife.FileStorage
{
    public record FileCategory
    {
        public string ServiceName { get; set; }
        public string FileFormName { get; set; } = "file";
        [Required(AllowEmptyStrings = false)]
        public string PathTemplate { get; set; } = "${category}/{now:yyyyMMdd}/{guid}${ext}";
        public Dictionary<string, object> Metadata { get; set; }
        public string[] Interceptors { get; set; }
        public long MaxLength { get; set; } = 4 * 1024 * 1024;
        public string[] AllowExtensions { get; set; }
        public string[] Callbacks { get; set; }
        public UploadMode Mode { get; set; } = UploadMode.Server;
    }
    public enum UploadMode
    {
        Server = 0,//通过服务端上传到存储服务
        Client = 1,//通过客户端直接上传到存储服务(不支持拦截器和回调)
    }

}

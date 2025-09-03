using System.ComponentModel.DataAnnotations;

namespace YS.Knife.FileStorage
{
    public record FileCategory
    {
        public string ServiceName { get; set; }
        public string FileFormName { get; set; } = "file";
        [Required(AllowEmptyStrings = false)]
        public string PathTemplate { get; set; }
        public Dictionary<string, object> Metadata { get; set; }
        public string[] Interceptors { get; set; }
        public long MaxLength { get; set; } = 4 * 1024 * 1024;
        public string[] AllowExtensions { get; set; }
    }

}

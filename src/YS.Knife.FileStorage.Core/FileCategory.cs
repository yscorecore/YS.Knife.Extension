using System.ComponentModel.DataAnnotations;

namespace YS.Knife.FileStorage
{
    public record FileCategory
    {
        public string FileFormName { get; set; } = "file";
        [Required(AllowEmptyStrings = false)]
        public string PathTemplate { get; set; }
        public Dictionary<string, object> Metadata { get; set; }
        public string[] Interceptors { get; set; }
    }

}

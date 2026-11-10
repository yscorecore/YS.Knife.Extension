
namespace YS.Knife.FileStorage
{
    public record ClientUploadInfo
    {
        public string UploadUrl { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public string FileFormName { get; set; }
        public string Method { get; set; }
    }
}

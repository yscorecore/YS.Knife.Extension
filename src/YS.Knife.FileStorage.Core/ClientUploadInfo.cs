
namespace YS.Knife.FileStorage
{
    public record ClientUploadInfo
    {
        public string Url { get; set; }
        public Dictionary<string, object> Headers { get; set; }
        public string FileFormName { get; set; }
        public string Method { get; set; }
    }
}

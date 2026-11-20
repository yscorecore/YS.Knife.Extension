namespace YS.Knife.FileStorage
{
    public record FileObject
    {
        public string Key { get; set; }
        public string FileName { get; set; }
        public string PublicUrl { get; set; }
    }
    public record FileUploadObject : FileObject
    {
        public long Size { get; set; }
        public string Extension { get; set; }
        public string OriginFileName { get; set; }
    }

}

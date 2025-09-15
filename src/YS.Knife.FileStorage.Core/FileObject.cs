namespace YS.Knife.FileStorage
{
    public record FileObject
    {
        public FileObject()
        {

        }
        public string Key { get; set; }
        public string FileName { get; set; }
        public string PublicUrl { get; set; }
    }

}

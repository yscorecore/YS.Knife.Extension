namespace YS.Knife.FileStorage
{
    public record FileCategory
    {
        public string Name { get; set; }
        public string FileFormName { get; set; } = "file";
        public string PathTemplate { get; set; }
        public Dictionary<string, object> Vars { get; set; }
        public Dictionary<string, object> Metadata { get; set; }

    }

}

namespace YS.Knife.Task
{
    public record class TaskDescription
    {
        public string Name { get; set; } = null!;
        public string? Version { get; set; }
        public string Description { get; set; } = null!;
        public string? Group { get; set; }
        public string? ArgumentMeta { get; set; }
    }
}

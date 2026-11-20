namespace YS.Knife.Function.Files
{
    public class ActionInfo
    {
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Desc { get; set; } = null!;
        public Dictionary<string, object>? Config { get; set; } = null!;
    }
}

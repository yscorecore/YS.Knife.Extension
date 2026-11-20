namespace YS.Knife.Function.Files
{
    public class ModuleInfo
    {
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Desc { get; set; } = null!;
        public Dictionary<string, object>? Config { get; set; } = null!;
        public List<ModuleInfo>? Modules { get; set; } = null!;
        public List<ActionInfo>? Actions { get; set; } = null!;
    }
}

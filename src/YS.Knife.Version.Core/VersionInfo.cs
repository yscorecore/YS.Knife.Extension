namespace YS.Knife.Version
{
    public record VersionInfo
    {
        public DateTimeOffset BuildTime { get; init; }
        public string AssemblyVersion { get; init; }
        public string AssemblyFileVersion { get; init; }
        public string AssemblyInformationalVersion { get; init; }
    }

}

using System.Reflection;

namespace YS.Knife.Version.Impl.Default
{
    [Service(Lifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton)]
    public class VersionService : IVersionService
    {
        public VersionInfo GetVersionInfo()
        {
            var entryAssembly = Assembly.GetEntryAssembly() ?? throw new Exception("Cannot get entry assembly");
            return new VersionInfo
            {
                BuildTime = entryAssembly.GetCustomAttribute<BuildTimeAttribute>()?.BuildTime ?? DateTime.UnixEpoch,
                AssemblyVersion = entryAssembly.GetCustomAttribute<AssemblyVersionAttribute>()?.Version,
                AssemblyFileVersion = entryAssembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version,
                AssemblyInformationalVersion = entryAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            };
        }
    }
}

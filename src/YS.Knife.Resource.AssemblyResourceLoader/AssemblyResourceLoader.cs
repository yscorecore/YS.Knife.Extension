using System.Reflection;
using System.Text.RegularExpressions;

namespace YS.Knife.Resource
{

    [Service(Lifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton)]
    public class AssemblyResourceLoader : IResourceLoader
    {
        static Regex UriRegex = new Regex(@"^assembly://(?<ass>\w+(\.\w+)*)/(?<res>.+)$");

        public int Priority => 2000;

        public bool CanLoad(string templateUri)
        {
            _ = templateUri ?? throw new ArgumentNullException(nameof(templateUri));
            return UriRegex.IsMatch(templateUri);
        }

        public Task<Stream> LoadTemplate(string templateUri)
        {
            var match = UriRegex.Match(templateUri);
            if (match.Success)
            {
                var assemblyName = match.Groups["ass"].Value;
                var resourceName = match.Groups["res"].Value;
                var assembly = Assembly.Load(assemblyName);
                return Task.FromResult(assembly.GetManifestResourceStream(resourceName));
            }
            else
            {
                throw new ArgumentException("invalid assembly loader format");
            }
        }
    }
}

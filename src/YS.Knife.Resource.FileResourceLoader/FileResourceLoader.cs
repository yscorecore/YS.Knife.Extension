namespace YS.Knife.Resource
{
    [Service(Lifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton)]
    public class FileResourceLoader : IResourceLoader
    {
        public int Priority => 3000;
        public bool CanLoad(string templateUri)
        {
            _ = templateUri ?? throw new ArgumentNullException(nameof(templateUri));
            if (Uri.IsWellFormedUriString(templateUri, UriKind.Absolute))
            {
                return false;
            }
            var invalidFileNameChars = Path.GetInvalidPathChars();
            return templateUri.All(p => !invalidFileNameChars.Contains(p));
        }

        public Task<Stream> LoadTemplate(string templateUri)
        {
            return Task.FromResult<Stream>(File.OpenRead(templateUri));
        }
    }
}

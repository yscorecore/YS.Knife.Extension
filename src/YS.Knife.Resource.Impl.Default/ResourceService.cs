namespace YS.Knife.Resource
{
    [Service]
    [AutoConstructor]
    public partial class ResourceService : IResourceService
    {
        private IEnumerable<IResourceLoader> loaders;

        public Task<Stream> Load(string resourceUri)
        {
            foreach (var templateLoader in loaders.OrderByDescending(p => p.Priority))
            {
                if (templateLoader.CanLoad(resourceUri))
                {
                    return templateLoader.LoadTemplate(resourceUri);
                }
            }
            throw Errors.UnknowTemplateUri(resourceUri);
        }
        [CodeExceptions]
        static partial class Errors
        {
            [CodeException("001", "Unknown template uri, '{uri}'")]
            public static partial Exception UnknowTemplateUri(string uri);
        }
    }
}

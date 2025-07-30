namespace YS.Knife.Resource
{
    public interface IResourceLoader
    {
        Task<Stream> LoadTemplate(string resourceUri);
        bool CanLoad(string resourceUri);
        int Priority { get; }
    }
}

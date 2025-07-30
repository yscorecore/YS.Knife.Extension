namespace YS.Knife.Resource
{
    public interface IResourceService
    {
        Task<Stream> Load(string resourceUri);
    }
}

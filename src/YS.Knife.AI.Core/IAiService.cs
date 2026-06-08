namespace YS.Knife.AI.Core
{
    public interface IAiService
    {
        Task<T> RecognizeImageAsObject<T>(string imageUrlOrBase64, string provider, string model, string prompt, CancellationToken cancellationToken = default)
            where T : class, new();
        Task<T[]> RecognizeImageAsArray<T>(string imageUrlOrBase64, string provider, string model, string prompt, CancellationToken cancellationToken = default)
            where T : class, new ();
    }
}

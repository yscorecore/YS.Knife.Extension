namespace YS.Knife.AI.Core
{
    public interface IAiProviderService
    {
        Task<string> RecognizeImage(string imageUrlOrBase64, string model, string prompt, CancellationToken cancellationToken = default);
    }
}

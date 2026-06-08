namespace YS.Knife.AI.Core
{
    public interface IAiProviderService
    {
        Task<string> RecognizeImage(AiInputData[] inputs, string model, string prompt, CancellationToken cancellationToken = default);
    }
}

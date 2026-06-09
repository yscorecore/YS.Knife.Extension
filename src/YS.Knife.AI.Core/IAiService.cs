namespace YS.Knife.AI.Core
{
    public interface IAiService
    {
        Task<T> RecognizeImageAsObject<T>(AiInputData[] inputs, string model, string prompt, CancellationToken cancellationToken = default)
            where T : class, new();
        Task<T[]> RecognizeImageAsArray<T>(AiInputData[] inputs, string model, string prompt, CancellationToken cancellationToken = default)
            where T : class, new();
    }

}

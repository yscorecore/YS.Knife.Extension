namespace YS.Knife.AI.Core
{
    public interface IAiService
    {
        Task<T> RecognizeImageAsObject<T>(AiInputData[] inputs, string model, string prompt, CancellationToken cancellationToken = default)
            where T : class, new();
        Task<T[]> RecognizeImageAsArray<T>(AiInputData[] inputs, string model, string prompt, CancellationToken cancellationToken = default)
            where T : class, new();


        Task<T> RecognizeDocumentAsObject<T>(AiInputData[] inputs, string model, string prompt, CancellationToken cancellationToken = default)
        where T : class, new();
        Task<T[]> RecognizeDocumentAsArray<T>(AiInputData[] inputs, string model, string prompt, CancellationToken cancellationToken = default)
            where T : class, new();


        Task<T> ChatAsObject<T>(string model, string prompt, CancellationToken cancellationToken = default)
            where T : class, new();
        Task<T[]> ChatAsArray<T>(string model, string prompt, CancellationToken cancellationToken = default)
            where T : class, new();

    }

}

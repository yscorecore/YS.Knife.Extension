namespace YS.Knife.Document
{
    public abstract class DocTypeConvertHandlerBase<TOptions> : IDocTypeConvertHandler
        where TOptions : class, new()
    {
        public abstract string Name { get; }
        public abstract StreamBody Convert(StreamBody input, TOptions options, CancellationToken token = default);

        public StreamBody Convert(StreamBody input, object options, CancellationToken token = default)
        {
            return Convert(input, options.AsJsonObject<TOptions>(IDocTypeConvertHandler.JsonOptions) ?? new TOptions(), token);
        }
    }
}

namespace YS.Knife.CodeExchange
{
    public abstract class BaseImageCodeHandler<TArg, TData> : IImageCodeHandler
    {
        public abstract string Name { get; }

        public abstract TimeSpan Expired { get; }

        public abstract ImageCodeDataKind DataKind { get; }

        public Task<(string Sence, Stream ImageStream)> GeneratorCode(object args, CancellationToken cancellationToken)
        {
            return GeneratorCode(args.AsJsonElement().AsJsonObject<TArg>(), cancellationToken);
        }
        public abstract Task<(string Sence, Stream ImageStream)> GeneratorCode(TArg args, CancellationToken cancellationToken);

        public Task OnDataPushed(object args, object data, CancellationToken cancellationToken)
        {
            return OnDataPushed(args.AsJsonElement().AsJsonObject<TArg>(IImageCodeHandler.JsonOptions), data.AsJsonElement().AsJsonObject<TData>(IImageCodeHandler.JsonOptions), cancellationToken);
        }
        public abstract Task OnDataPushed(TArg args, TData data, CancellationToken cancellationToken);
    }
}

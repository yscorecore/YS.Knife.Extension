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

        public async Task<object> ProcessData(object args, object userInputData, CancellationToken cancellationToken)
        {
            var res = await OnProcessData(args.AsJsonElement().AsJsonObject<TArg>(IImageCodeHandler.JsonOptions), userInputData.AsJsonElement().AsJsonObject<TData>(IImageCodeHandler.JsonOptions), cancellationToken);
            return res!;
        }
        public virtual Task<TData> OnProcessData(TArg args, TData data, CancellationToken cancellationToken)
        {
            return Task.FromResult(data);
        }
    }
}

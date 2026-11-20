using static YS.Knife.CodeExchange.IImageCodeHandler;

namespace YS.Knife.CodeExchange
{
    public abstract class BaseImageCodeHandler<TArg, TData> : IImageCodeHandler
    {
        public abstract string Name { get; }

        public abstract TimeSpan ExpiresIn { get; }

        public abstract ImageCodeDataKind DataKind { get; }

        public Task<(string Sence, Stream ImageStream)> GeneratorCode(object args, CancellationToken cancellationToken)
        {
            return GeneratorCode(args.AsJsonObject<TArg>(JsonOptions), cancellationToken);
        }
        public abstract Task<(string Sence, Stream ImageStream)> GeneratorCode(TArg args, CancellationToken cancellationToken);

        public virtual Task<ArgDataPair> OnProcessData(TArg args, TData data, CancellationToken cancellationToken)
        {
            return Task.FromResult(new ArgDataPair(args, data));
        }

        public async Task<IImageCodeHandler.ArgDataPair> ProcessData(object args, object data, CancellationToken cancellationToken)
        {
            var (arg, dataObj) = await OnProcessData(
                args.AsJsonObject<TArg>(JsonOptions),
                data.AsJsonObject<TData>(JsonOptions), cancellationToken);
            return new IImageCodeHandler.ArgDataPair(arg!, dataObj!);
        }

        public record ArgDataPair(TArg Args, TData Data);

    }
}

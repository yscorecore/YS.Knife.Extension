namespace YS.Knife.CodeExchange
{
    public interface IImageCodeHandler
    {
        string Name { get; }
        TimeSpan Expired { get; }
        ImageCodeDataKind DataKind { get; }
        Task<(string Sence, Stream ImageStream)> GeneratorCode(object args, CancellationToken cancellationToken);
        Task OnDataPushed(object args, object data, CancellationToken cancellationToken);
    }
}

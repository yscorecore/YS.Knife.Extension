namespace YS.Knife.Metadata
{
    public interface IMetadataInterceptor
    {
        int Priority { get; }
        string Name { get; }
        bool IsGlobal { get => string.IsNullOrEmpty(Name); }
        Task Process(MetadataInterceptorContext context, CancellationToken cancellationToken);
    }
}

namespace YS.Knife.CodeMapper
{
    public interface ICodeMapperService
    {
        Task<List<MapperResult<TData>>> MapAll<TData, TSourceKey>(IEnumerable<TData> source, string group, Func<TData, TSourceKey> sourceCodeFun, Func<TData, string>? sourceNameFun = null, bool autoInsertSourceData = true, CancellationToken token = default);
    }
    public static class CodeMapperServiceExtensions
    {
        public static async Task<MapperResult<TData>> MapOne<TData, TSourceKey, TTargetKey>(this ICodeMapperService service, TData data, string group, Func<TData, TSourceKey> sourceKeyFun, CancellationToken token = default)
        {
            var res = await service.MapAll(new TData[] { data }, group, sourceKeyFun, null, false, token);
            return res.Single();
        }
    }

    public record MapperResult<T>(
        T Data,
        bool Mapped,
        string? TargetCode,
        string? TargetName);

}

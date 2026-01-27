namespace YS.Knife.CodeMapper
{
    public record MapperResult<T>(
        T Data,
        bool Mapped,
        string? TargetCode,
        string? TargetName);

}

namespace YS.Knife.TokenMan
{
    public interface ITokenManager
    {
        Task<TokenInfo<T>> GetToken<T>(string name);
    }
    public record TokenInfo<T>
    {
         public T Token { get; set; }
    }
}

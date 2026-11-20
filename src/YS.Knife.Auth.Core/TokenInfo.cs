namespace YS.Knife.Auth
{
    public sealed record TokenInfo(
        string TokenType,
        string AccessToken,
        long ExpiresIn,
        string RefreshToken);
}

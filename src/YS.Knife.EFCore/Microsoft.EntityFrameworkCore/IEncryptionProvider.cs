namespace Microsoft.EntityFrameworkCore
{
    public interface IEncryptionProvider
    {
        string Encrypt(string plainText);
        string Decrypt(string cipherText);
        int GetEncryptedLength(int maxPlainTextLength);
    }
}

namespace Microsoft.EntityFrameworkCore
{
    public interface IEncryptionProvider
    {
        void SetKey(string key);
        string Encrypt(string plainText);
        string Decrypt(string cipherText);
        int GetEncryptedLength(int maxPlainTextLength);
    }
}

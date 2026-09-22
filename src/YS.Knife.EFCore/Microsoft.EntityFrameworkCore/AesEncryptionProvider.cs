using System.Security.Cryptography;
using System.Text;

namespace Microsoft.EntityFrameworkCore
{
    public class AesEncryptionProvider : IEncryptionProvider
    {
        private readonly Aes aes;

        public AesEncryptionProvider(string passphrase)
        {
            aes = Aes.Create();
            aes.Key = SHA256.HashData(Encoding.UTF8.GetBytes(passphrase));
        }

        public string Encrypt(string plainText)
        {
            var iv = RandomNumberGenerator.GetBytes(16);
            using var encryptor = aes.CreateEncryptor(aes.Key, iv);

            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            var result = new byte[16 + cipherBytes.Length];
            Buffer.BlockCopy(iv, 0, result, 0, 16);
            Buffer.BlockCopy(cipherBytes, 0, result, 16, cipherBytes.Length);

            return Convert.ToBase64String(result);
        }

        public string Decrypt(string cipherText)
        {
            var fullBytes = Convert.FromBase64String(cipherText);

            var iv = new byte[16];
            Buffer.BlockCopy(fullBytes, 0, iv, 0, 16);

            using var decryptor = aes.CreateDecryptor(aes.Key, iv);
            var plainBytes = decryptor.TransformFinalBlock(fullBytes, 16, fullBytes.Length - 16);

            return Encoding.UTF8.GetString(plainBytes);
        }

        public int GetEncryptedLength(int maxPlainTextLength)
        {
            var maxBytes = Encoding.UTF8.GetMaxByteCount(maxPlainTextLength);
            var blockSize = 16;
            var paddedSize = ((maxBytes / blockSize) + 1) * blockSize;
            var rawSize = blockSize + paddedSize;
            return ((rawSize + 2) / 3) * 4;
        }
    }
}

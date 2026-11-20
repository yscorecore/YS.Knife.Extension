using System.Security.Cryptography;
using System.Text;

namespace Microsoft.EntityFrameworkCore
{
    public class SymmetricEncryptionProvider : IEncryptionProvider
    {
        private readonly SymmetricAlgorithm algorithm;
        private readonly string passwordKeyName;
        private bool keySet;

        public SymmetricEncryptionProvider(string algorithmName, string passwordKey)
        {
            algorithm = SymmetricAlgorithm.Create(algorithmName)
                ?? throw new ArgumentException($"Unsupported symmetric algorithm: {algorithmName}");
            this.passwordKeyName = passwordKey;
        }

        public void SetKey(string key)
        {
            var keyBytes = SHA256.HashData(Encoding.UTF8.GetBytes(key));
            var maxKeyBytes = algorithm.LegalKeySizes.Max(s => s.MaxSize) / 8;
            if (keyBytes.Length > maxKeyBytes)
            {
                var trimmed = new byte[maxKeyBytes];
                Buffer.BlockCopy(keyBytes, 0, trimmed, 0, maxKeyBytes);
                keyBytes = trimmed;
            }
            algorithm.Key = keyBytes;
            keySet = true;
        }

        public string Encrypt(string plainText)
        {
            EnsureKeySet();
            var blockSize = algorithm.BlockSize / 8;
            var iv = RandomNumberGenerator.GetBytes(blockSize);
            using var encryptor = algorithm.CreateEncryptor(algorithm.Key, iv);

            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            var result = new byte[blockSize + cipherBytes.Length];
            Buffer.BlockCopy(iv, 0, result, 0, blockSize);
            Buffer.BlockCopy(cipherBytes, 0, result, blockSize, cipherBytes.Length);

            return Convert.ToBase64String(result);
        }

        public string Decrypt(string cipherText)
        {
            EnsureKeySet();
            var fullBytes = Convert.FromBase64String(cipherText);
            var blockSize = algorithm.BlockSize / 8;

            var iv = new byte[blockSize];
            Buffer.BlockCopy(fullBytes, 0, iv, 0, blockSize);

            using var decryptor = algorithm.CreateDecryptor(algorithm.Key, iv);
            var plainBytes = decryptor.TransformFinalBlock(fullBytes, blockSize, fullBytes.Length - blockSize);

            return Encoding.UTF8.GetString(plainBytes);
        }

        public int GetEncryptedLength(int maxPlainTextLength)
        {
            var maxBytes = Encoding.UTF8.GetMaxByteCount(maxPlainTextLength);
            var blockSize = algorithm.BlockSize / 8;
            var paddedSize = ((maxBytes / blockSize) + 1) * blockSize;
            var rawSize = blockSize + paddedSize;
            return ((rawSize + 2) / 3) * 4;
        }

        private void EnsureKeySet()
        {
            if (!keySet)
            {
                lock (this)
                {
                    if (!keySet)
                    {
                        if (!EncryptionKeys.TryGet(passwordKeyName, out var passwordKey))
                            throw new InvalidOperationException(
                                $"Encryption key '{passwordKeyName}' not found. Call EncryptionKeys.Set(\"{passwordKeyName}\", ...) before using encryption.");
                        var keyBytes = SHA256.HashData(Encoding.UTF8.GetBytes(passwordKey));
                        var maxKeyBytes = algorithm.LegalKeySizes.Max(s => s.MaxSize) / 8;
                        if (keyBytes.Length > maxKeyBytes)
                        {
                            var trimmed = new byte[maxKeyBytes];
                            Buffer.BlockCopy(keyBytes, 0, trimmed, 0, maxKeyBytes);
                            keyBytes = trimmed;
                        }
                        algorithm.Key = keyBytes;
                        keySet = true;
                    }
                }
            }
        }
    }
}

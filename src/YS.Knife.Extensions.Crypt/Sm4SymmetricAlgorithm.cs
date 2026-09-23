using System.Security.Cryptography;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace YS.Knife.Extensions.Crypt
{
    public class Sm4SymmetricAlgorithm : SymmetricAlgorithm
    {
        public Sm4SymmetricAlgorithm()
        {
            KeySizeValue = 128;
            BlockSizeValue = 128;
            FeedbackSizeValue = 128;
            ModeValue = CipherMode.CBC;
            PaddingValue = PaddingMode.PKCS7;
            LegalBlockSizesValue = new[] { new KeySizes(128, 128, 0) };
            LegalKeySizesValue = new[] { new KeySizes(128, 128, 0) };
        }

        public override ICryptoTransform CreateEncryptor(byte[] rgbKey, byte[] rgbIV)
            => new Sm4CryptoTransform(rgbKey, rgbIV, true);

        public override ICryptoTransform CreateDecryptor(byte[] rgbKey, byte[] rgbIV)
            => new Sm4CryptoTransform(rgbKey, rgbIV, false);

        public override void GenerateKey()
        {
            Key = RandomNumberGenerator.GetBytes(16);
        }

        public override void GenerateIV()
        {
            IV = RandomNumberGenerator.GetBytes(16);
        }

        private class Sm4CryptoTransform : ICryptoTransform
        {
            private readonly IBufferedCipher cipher;

            public Sm4CryptoTransform(byte[] key, byte[] iv, bool encrypting)
            {
                cipher = CipherUtilities.GetCipher("SM4/CBC/PKCS7Padding");
                var ivOrZero = iv ?? new byte[16];
                var parameters = new ParametersWithIV(new KeyParameter(key), ivOrZero);
                cipher.Init(encrypting, parameters);

                InputBlockSize = 16;
                OutputBlockSize = 16;
                CanTransformMultipleBlocks = true;
                CanReuseTransform = false;
            }

            public int InputBlockSize { get; }
            public int OutputBlockSize { get; }
            public bool CanTransformMultipleBlocks { get; }
            public bool CanReuseTransform { get; }

            public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount,
                byte[] outputBuffer, int outputOffset)
            {
                var processed = cipher.ProcessBytes(inputBuffer, inputOffset, inputCount);
                if (processed.Length > 0)
                {
                    Buffer.BlockCopy(processed, 0, outputBuffer, outputOffset, processed.Length);
                }
                return processed.Length;
            }

            public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
            {
                var processed = cipher.ProcessBytes(inputBuffer, inputOffset, inputCount);
                var final = cipher.DoFinal();

                var result = new byte[processed.Length + final.Length];
                if (processed.Length > 0)
                    Buffer.BlockCopy(processed, 0, result, 0, processed.Length);
                if (final.Length > 0)
                    Buffer.BlockCopy(final, 0, result, processed.Length, final.Length);
                return result;
            }

            public void Dispose() { }
        }
    }
}

using System.Security.Cryptography;

namespace YS.Knife.Extensions.Crypt
{
    public class Sm4SymmetricAlgorithm : SymmetricAlgorithm
    {
        private static readonly byte[] SBox = new byte[]
        {
            0xD6,0x90,0xE9,0xFE,0xCC,0xE1,0x3D,0xB7,0x16,0xB6,0x14,0xC2,0x28,0xFB,0x2C,0x05,
            0x2B,0x67,0x9A,0x76,0x2A,0xBE,0x04,0xC3,0xAA,0x44,0x13,0x26,0x49,0x86,0x06,0x99,
            0x9C,0x42,0x50,0xF4,0x91,0xEF,0x98,0x7A,0x33,0x54,0x0B,0x43,0xED,0xCF,0xAC,0x62,
            0xE4,0xB3,0x1C,0xA9,0xC9,0x08,0xE8,0x95,0x80,0xDF,0x94,0xFA,0x75,0x8F,0x3F,0xA6,
            0x47,0x07,0xA7,0xFC,0xF3,0x73,0x17,0xBA,0x83,0x59,0x3C,0x19,0xE6,0x85,0x4F,0xA8,
            0x68,0x6B,0x81,0xB2,0x71,0x64,0xDA,0x8B,0xF8,0xEB,0x0F,0x4B,0x70,0x56,0x9D,0x35,
            0x1E,0x24,0x0E,0x5E,0x63,0x58,0xD1,0xA2,0x25,0x22,0x7C,0x3B,0x01,0x21,0x78,0x87,
            0xD4,0x00,0x46,0x57,0x9F,0xD3,0x27,0x52,0x4C,0x36,0x02,0xE7,0xA0,0xC4,0xC8,0x9E,
            0xEA,0xBF,0x8A,0xD2,0x40,0xC7,0x38,0xB5,0xA3,0xF7,0xF2,0xCE,0xF9,0x61,0x15,0xA1,
            0xE0,0xAE,0x5D,0xA4,0x9B,0x34,0x1A,0x55,0xAD,0x93,0x32,0x30,0xF5,0x8C,0xB1,0xE3,
            0x1D,0xF6,0xE2,0x2E,0x82,0x66,0xCA,0x60,0xC0,0x29,0x23,0xAB,0x0D,0x53,0x4E,0x6F,
            0xD5,0xDB,0x37,0x45,0xDE,0xFD,0x8E,0x2F,0x03,0xFF,0x6A,0x72,0x6D,0x6C,0x5B,0x51,
            0x8D,0x1B,0xAF,0x92,0xBB,0xDD,0xBC,0x7F,0x11,0xD9,0x5C,0x41,0x1F,0x10,0x5A,0xD8,
            0x0A,0xC1,0x31,0x88,0xA5,0xCD,0x7B,0xBD,0x2D,0x74,0xD0,0x12,0xB8,0xE5,0xB4,0xB0,
            0x89,0x69,0x97,0x4A,0x0C,0x96,0x77,0x7E,0x65,0xB9,0xF1,0x09,0xC5,0x6E,0xC6,0x84,
            0x18,0xF0,0x7D,0xEC,0x3A,0xDC,0x4D,0x20,0x79,0xEE,0x5F,0x3E,0xD7,0xCB,0x39,0x48,
        };

        private static readonly uint[] FK = new uint[]
        {
            0xB3C2D5E6, 0x14A3F567, 0x9568BA79, 0x0F2CE3D1
        };

        private static readonly uint[] CK = new uint[]
        {
            0x00070E01, 0x080F0209, 0x1017040B, 0x181F0C0D,
            0x20270E0F, 0x282F1617, 0x30371819, 0x383F1E1F,
            0x40472021, 0x484F2627, 0x50572829, 0x585F2E2F,
            0x60673031, 0x686F3637, 0x70773839, 0x787F3E3F,
            0x80874041, 0x888F4647, 0x90974849, 0x989F4E4F,
            0xA0A75051, 0xA8AF5657, 0xB0B75859, 0xB8BF5E5F,
            0xC0C76061, 0xC8CF6667, 0xD0D76869, 0xD8DF6E6F,
            0xE0E77071, 0xE8EF7677, 0xF0F77879, 0xF8FF7E7F,
        };

        public Sm4SymmetricAlgorithm()
        {
            KeySizeValue = 128;
            BlockSizeValue = 128;
            FeedbackSizeValue = 128;
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

        private static uint BytesToUInt32(byte[] src, int offset)
        {
            return ((uint)src[offset] << 24)
                 | ((uint)src[offset + 1] << 16)
                 | ((uint)src[offset + 2] << 8)
                 | src[offset + 3];
        }

        private static void UInt32ToBytes(uint value, byte[] dst, int offset)
        {
            dst[offset] = (byte)(value >> 24);
            dst[offset + 1] = (byte)(value >> 16);
            dst[offset + 2] = (byte)(value >> 8);
            dst[offset + 3] = (byte)value;
        }

        private static uint Tau(uint a)
        {
            return ((uint)SBox[(a >> 24) & 0xFF] << 24)
                 | ((uint)SBox[(a >> 16) & 0xFF] << 16)
                 | ((uint)SBox[(a >> 8) & 0xFF] << 8)
                 | SBox[a & 0xFF];
        }

        private static uint L(uint b)
        {
            return b ^ RotateLeft(b, 2) ^ RotateLeft(b, 10) ^ RotateLeft(b, 18) ^ RotateLeft(b, 24);
        }

        private static uint LPrime(uint b)
        {
            return b ^ RotateLeft(b, 13) ^ RotateLeft(b, 23);
        }

        private static uint RotateLeft(uint x, int n)
        {
            return (x << n) | (x >> (32 - n));
        }

        private static uint F(uint x0, uint x1, uint x2, uint x3, uint rk)
        {
            return x0 ^ L(Tau(x1 ^ x2 ^ x3 ^ rk));
        }

        private static uint[] DeriveRoundKeys(byte[] key)
        {
            var mk = new uint[]
            {
                BytesToUInt32(key, 0),
                BytesToUInt32(key, 4),
                BytesToUInt32(key, 8),
                BytesToUInt32(key, 12),
            };

            var k = new uint[36];
            k[0] = mk[0] ^ FK[0];
            k[1] = mk[1] ^ FK[1];
            k[2] = mk[2] ^ FK[2];
            k[3] = mk[3] ^ FK[3];

            var rk = new uint[32];
            for (int i = 0; i < 32; i++)
            {
                k[i + 4] = k[i] ^ LPrime(Tau(k[i + 1] ^ k[i + 2] ^ k[i + 3] ^ CK[i]));
                rk[i] = k[i + 4];
            }

            return rk;
        }

        private static void ProcessBlock(uint[] rk, byte[] input, int inputOffset, byte[] output, int outputOffset)
        {
            var x = new uint[36];
            x[0] = BytesToUInt32(input, inputOffset);
            x[1] = BytesToUInt32(input, inputOffset + 4);
            x[2] = BytesToUInt32(input, inputOffset + 8);
            x[3] = BytesToUInt32(input, inputOffset + 12);

            for (int i = 0; i < 32; i++)
            {
                x[i + 4] = F(x[i], x[i + 1], x[i + 2], x[i + 3], rk[i]);
            }

            UInt32ToBytes(x[35], output, outputOffset);
            UInt32ToBytes(x[34], output, outputOffset + 4);
            UInt32ToBytes(x[33], output, outputOffset + 8);
            UInt32ToBytes(x[32], output, outputOffset + 12);
        }

        private class Sm4CryptoTransform : ICryptoTransform
        {
            private readonly uint[] roundKeys;
            private readonly bool encrypting;

            public Sm4CryptoTransform(byte[] key, byte[] iv, bool encrypting)
            {
                roundKeys = DeriveRoundKeys(key);
                this.encrypting = encrypting;
                InputBlockSize = 16;
                OutputBlockSize = 16;
                CanTransformMultipleBlocks = true;
                CanReuseTransform = true;
            }

            public int InputBlockSize { get; }
            public int OutputBlockSize { get; }
            public bool CanTransformMultipleBlocks { get; }
            public bool CanReuseTransform { get; }

            public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount,
                byte[] outputBuffer, int outputOffset)
            {
                var rk = encrypting ? roundKeys : ReversedRoundKeys();
                for (int i = 0; i < inputCount; i += 16)
                {
                    ProcessBlock(rk, inputBuffer, inputOffset + i, outputBuffer, outputOffset + i);
                }
                return inputCount;
            }

            public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
            {
                if (encrypting)
                {
                    int paddedLength = ((inputCount / 16) + 1) * 16;
                    var padded = new byte[paddedLength];
                    Buffer.BlockCopy(inputBuffer, inputOffset, padded, 0, inputCount);
                    byte padValue = (byte)(paddedLength - inputCount);
                    for (int i = inputCount; i < paddedLength; i++)
                        padded[i] = padValue;

                    var output = new byte[paddedLength];
                    var rk = roundKeys;
                    for (int i = 0; i < paddedLength; i += 16)
                    {
                        ProcessBlock(rk, padded, i, output, i);
                    }
                    return output;
                }
                else
                {
                    if (inputCount == 0) return Array.Empty<byte>();

                    var output = new byte[inputCount];
                    var rk = ReversedRoundKeys();
                    for (int i = 0; i < inputCount; i += 16)
                    {
                        ProcessBlock(rk, inputBuffer, inputOffset + i, output, i);
                    }

                    byte padValue = output[output.Length - 1];
                    if (padValue >= 1 && padValue <= 16)
                    {
                        int unpaddedLength = output.Length - padValue;
                        var result = new byte[unpaddedLength];
                        Buffer.BlockCopy(output, 0, result, 0, unpaddedLength);
                        return result;
                    }
                    return output;
                }
            }

            private uint[] ReversedRoundKeys()
            {
                var reversed = new uint[roundKeys.Length];
                for (int i = 0; i < roundKeys.Length; i++)
                    reversed[i] = roundKeys[roundKeys.Length - 1 - i];
                return reversed;
            }

            public void Dispose() { }
        }
    }
}

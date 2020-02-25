using System;
using System.Linq;

namespace PasswordsManager.Cryptography
{

    public sealed class Rijndael
    {

        #region Constants

        private const int ColumnHeight = 4;

        #endregion

        #region Static fields
        
        private static Lazy<byte[]> _mixColumnsPolynomial;
        private static Lazy<byte[]> _invertedMixColumnsPolynomial;
        private static Lazy<byte[]> _rCon;

        #endregion

        #region Fields

        private GF256 _GF256;
        private RijndaelBlockSizes _blockSize;
        private RijndaelKeySizes _keySize;
        private byte[] _key;
        private Lazy<byte[][]> _roundKeys;
        private Lazy<byte[]> _sBox;
        private Lazy<byte[]> _invertedSBox;

        #endregion

        #region Constructors

        static Rijndael()
        {
            _mixColumnsPolynomial = new Lazy<byte[]>(CreateMixColumnsPolynomial, true);
            _invertedMixColumnsPolynomial = new Lazy<byte[]>(CreateInvertedMixColumnsPolynomial, true);
            _rCon = new Lazy<byte[]>(CreateRCon, true);
        }

        public Rijndael(RijndaelBlockSizes blockSize, RijndaelKeySizes keySize, byte irreduciblePolynomial, byte[] key)
        {
            BlockSize = blockSize;
            KeySize = keySize;
            GF256 = new GF256(irreduciblePolynomial);
            Key = key;
            _sBox = new Lazy<byte[]>(CreateSBox, true);
            _invertedSBox = new Lazy<byte[]>(CreateInvertedSBox, true);
        }

        #endregion

        #region Static properties

        private static byte[] MixColumnsPolynomial =>
            _mixColumnsPolynomial.Value;

        private static byte[] InvertedMixColumnsPolynomial =>
            _invertedMixColumnsPolynomial.Value;

        private static byte[] RCon =>
            _rCon.Value;

        #endregion

        #region Properties

        private GF256 GF256
        {
            get =>
                _GF256 ?? throw new ArgumentNullException(nameof(GF256));

            set =>
                _GF256 = value;
        }

        internal RijndaelBlockSizes BlockSize
        {
            get =>
                _blockSize;

            set
            {
                if (!Enum.IsDefined(typeof(RijndaelBlockSizes), value))
                {
                    throw new ArgumentException("Invalid block length.", nameof(value));
                }
                _blockSize = value;
            }
        }

        internal RijndaelKeySizes KeySize
        {
            get =>
                _keySize;

            set
            {
                if (!Enum.IsDefined(typeof(RijndaelKeySizes), value))
                {
                    throw new ArgumentException("Invalid key length.", nameof(value));
                }
                _keySize = value;
            }
        }

        private int ColumnsCount =>
            (int)BlockSize / ColumnHeight;

        private byte[] SBox =>
            _sBox.Value;

        private byte[] InvertedSBox =>
            _invertedSBox.Value;

        private byte[] Key
        {
            get =>
                _key;

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(Key));
                }
                if (value.Length != (int)KeySize)
                {
                    throw new ArgumentException("Invalid Rijndael key length.", nameof(Key));
                }
                _key = (byte[])value.Clone();
                _roundKeys = new Lazy<byte[][]>(CreateRoundKeys, true);
            }
        }

        private byte[][] RoundKeys =>
            _roundKeys.Value;

        private int RoundsCount
        {
            get
            {
                if ((int)BlockSize == 16 && Key.Length == 16)
                {
                    return 10;
                }
                if ((int)BlockSize == 32 || Key.Length == 32)
                {
                    return 14;
                }
                return 12;
            }
        }

        #endregion

        #region Private methods

        private void AddRoundKey(byte[] state, byte[] roundKey)
        {
            for (var i = 0; i < Math.Max(state.Length, roundKey.Length); i++)
            {
                state[i % state.Length] ^= roundKey[i % roundKey.Length];
            }
        }

        private void SubBytes(byte[] state)
        {
            for (var i = 0; i < (int)BlockSize; i++)
            {
                state[i] = SBox[state[i]];
            }
        }

        private void ShiftRows(byte[] state)
        {
            var temporary = state[1];
            var additionalShift = (int)BlockSize == 32 ? 1 : 0;
            for (var i = 0; i < ColumnsCount - 1; i++)
            {
                state[i * 4 + 1] = state[(i + 1) * 4 + 1];
            }
            state[(int)BlockSize - 3] = temporary;
            for (var i = 0; i < 2 + additionalShift; i++)
            {
                temporary = state[2];
                for (var j = 0; j < ColumnsCount - 1; j++)
                {
                    state[j * 4 + 2] = state[(j + 1) * 4 + 2];
                }
                state[(int)BlockSize - 2] = temporary;
            }
            for (var i = 0; i < 3 + additionalShift; i++)
            {
                temporary = state[3];
                for (var j = 0; j < ColumnsCount - 1; j++)
                {
                    state[j * 4 + 3] = state[(j + 1) * 4 + 3];
                }
                state[(int)BlockSize - 1] = temporary;
            }
        }

        private void MixColumns(byte[] state)
        {
            var stateWithMixedColumns = new byte[(int)BlockSize];
            for (var i = 0; i < ColumnsCount; i++)
            {
                for (var j = 0; j < 4; j++)
                {
                    for (var k = 0; k < 4; k++)
                    {
                        stateWithMixedColumns[i * 4 + k] = GF256.Add(stateWithMixedColumns[i * 4 + k]
                            , GF256.Multiply(MixColumnsPolynomial[j], state[i * 4 + (3 + k - j) % 4]));
                    }
                }
            }
            for (var i = 0; i < (int)BlockSize; i++)
            {
                state[i] = stateWithMixedColumns[i];
            }
        }

        private void InvertedSubBytes(byte[] state)
        {
            for (var i = 0; i < (int)BlockSize; i++)
            {
                state[i] = InvertedSBox[state[i]];
            }
        }

        private void InvertedShiftRows(byte[] state)
        {
            var temporary = state[(int)BlockSize - 3];
            var additionalShift = (int)BlockSize == 32 ? 1 : 0;
            for (var i = ColumnsCount - 1; i > 0; i--)
            {
                state[i * 4 + 1] = state[(i - 1) * 4 + 1];
            }
            state[1] = temporary;
            for (var i = 0; i < 2 + additionalShift; i++)
            {
                temporary = state[(int)BlockSize - 2];
                for (var j = ColumnsCount - 1; j > 0; j--)
                {
                    state[j * 4 + 2] = state[(j - 1) * 4 + 2];
                }
                state[2] = temporary;
            }
            for (var i = 0; i < 3 + additionalShift; i++)
            {
                temporary = state[(int)BlockSize - 1];
                for (var j = ColumnsCount - 1; j > 0; j--)
                {
                    state[j * 4 + 3] = state[(j - 1) * 4 + 3];
                }
                state[3] = temporary;
            }
        }

        private void InvertedMixColumns(byte[] state)
        {
            var stateWithInvertedMixedColumns = new byte[(int)BlockSize];
            for (var i = 0; i < ColumnsCount; i++)
            {
                for (var j = 0; j < 4; j++)
                {
                    for (var k = 0; k < 4; k++)
                    {
                        stateWithInvertedMixedColumns[i * 4 + k] = GF256.Add(stateWithInvertedMixedColumns[i * 4 + k]
                            , GF256.Multiply(InvertedMixColumnsPolynomial[j], state[i * 4 + (3 + k - j) % 4]));
                    }
                }
            }
            for (var i = 0; i < (int)BlockSize; i++)
            {
                state[i] = stateWithInvertedMixedColumns[i];
            }
        }

        #endregion

        #region Public methods

        public byte[] Encrypt(byte[] dataToEncrypt)
        {
            if (dataToEncrypt == null)
            {
                throw new ArgumentNullException(nameof(dataToEncrypt));
            }
            if (dataToEncrypt.Length != (int)BlockSize)
            {
                throw new ArgumentException("Invalid block length.", nameof(dataToEncrypt));
            }
            AddRoundKey(dataToEncrypt, RoundKeys.First());
            foreach (var round in Enumerable.Range(1, RoundsCount))
            {
                SubBytes(dataToEncrypt);
                ShiftRows(dataToEncrypt);
                if (round != RoundsCount)
                {
                    MixColumns(dataToEncrypt);
                }
                AddRoundKey(dataToEncrypt, RoundKeys[round]);
            }
            return dataToEncrypt;
        }

        public byte[] Decrypt(byte[] dataToDecrypt)
        {
            if (dataToDecrypt == null)
            {
                throw new ArgumentNullException(nameof(dataToDecrypt));
            }
            if (dataToDecrypt.Length != (int)BlockSize)
            {
                throw new ArgumentException("Invalid block length.", nameof(dataToDecrypt));
            }
            AddRoundKey(dataToDecrypt, RoundKeys.Last());
            foreach (var round in Enumerable.Range(0, RoundsCount).Reverse())
            {
                InvertedShiftRows(dataToDecrypt);
                InvertedSubBytes(dataToDecrypt);
                AddRoundKey(dataToDecrypt, RoundKeys[round]);
                if (round != 0)
                {
                    InvertedMixColumns(dataToDecrypt);
                }
            }
            return dataToDecrypt;
        }

        #endregion

        #region Lazy<T> private initialization methods

        private static byte[] CreateMixColumnsPolynomial()
        {
            return new byte[]
            {
                0x03, 0x01, 0x01, 0x02
            };
        }

        private static byte[] CreateInvertedMixColumnsPolynomial()
        {
            return new byte[]
            {
                0x0b, 0x0d, 0x09, 0x0e
            };
        }

        private static byte[] CreateRCon()
        {
            return new byte[]
            {
                0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80, 0x1b, 0x36
            };
        }

        private byte[] CreateSBox()
        {
            var sBox = new byte[256];
            for (byte i = 0; i < sBox.Length; i++)
            {
                var multiplicativeInverted = GF256.Invert(i);
                sBox[i] = GF256.Add(sBox[i], multiplicativeInverted);
                sBox[i] = GF256.Add(sBox[i], (byte)((multiplicativeInverted << 1) | (multiplicativeInverted >> 7)));
                sBox[i] = GF256.Add(sBox[i], (byte)((multiplicativeInverted << 2) | (multiplicativeInverted >> 6)));
                sBox[i] = GF256.Add(sBox[i], (byte)((multiplicativeInverted << 3) | (multiplicativeInverted >> 5)));
                sBox[i] = GF256.Add(sBox[i], (byte)((multiplicativeInverted << 4) | (multiplicativeInverted >> 4)));
                sBox[i] = GF256.Add(sBox[i], 0x63);
            }
            return sBox;
        }

        private byte[] CreateInvertedSBox()
        {
            var invertedSBox = new byte[256];
            for (byte i = 0; i < invertedSBox.Length; i++)
            {
                invertedSBox[i] = GF256.Add(invertedSBox[i], 0x05);
                invertedSBox[i] = GF256.Add(invertedSBox[i], (byte)((i << 1) | (i >> 7)));
                invertedSBox[i] = GF256.Add(invertedSBox[i], (byte)((i << 3) | (i >> 5)));
                invertedSBox[i] = GF256.Add(invertedSBox[i], (byte)((i << 6) | (i >> 2)));
                invertedSBox[i] = GF256.Invert(invertedSBox[i]);
            }
            return invertedSBox;
        }

        private byte[][] CreateRoundKeys()
        {
            var roundKeys = new byte[RoundsCount + 1][];
            roundKeys[0] = new byte[(int)BlockSize];
            for (var i = 0; i < (int)BlockSize; i++)
            {
                roundKeys[0][i] = Key[i % Key.Length];
            }
            for (var i = 1; i <= RoundsCount; i++)
            {
                roundKeys[i] = new byte[(int)BlockSize];
                var W = new byte[4]
                {
                    roundKeys[i - 1][roundKeys[i - 1].Length - 3]
                    , roundKeys[i - 1][roundKeys[i - 1].Length - 2]
                    , roundKeys[i - 1][roundKeys[i - 1].Length - 1]
                    , roundKeys[i - 1][roundKeys[i - 1].Length - 4]
                };
                roundKeys[i][0] = (byte)(roundKeys[i - 1][0] ^ SBox[W[0]] ^ RCon[(i - 1) % RCon.Length]);
                for (var j = 1; j < 4; j++)
                {
                    roundKeys[i][j] = (byte)(roundKeys[i - 1][j] ^ SBox[W[j]]);
                }
                for (var j = 1; j < ColumnsCount; j++)
                {
                    for (var k = 0; k < 4; k++)
                    {
                        roundKeys[i][j * 4 + k] = (byte)(roundKeys[i - 1][j * 4 + k] ^ roundKeys[i][(j - 1) * 4 + k]);
                    }
                }
            }
            return roundKeys;
        }

        #endregion

    }

}
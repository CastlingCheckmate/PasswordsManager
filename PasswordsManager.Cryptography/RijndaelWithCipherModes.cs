using System;

namespace PasswordsManager.Cryptography
{

    public sealed class RijndaelWithCipherModes
    {

        #region Fields

        private Rijndael _rijndael;
        private SymmetricCipherModes _cipherMode;
        private byte[] _IV;

        #endregion

        #region Constructors

        public RijndaelWithCipherModes(RijndaelBlockSizes blockSize, RijndaelKeySizes keySize, byte irreduciblePolynomial, SymmetricCipherModes cipherMode, byte[] key, byte[] IV)
        {
            Rijndael = new Rijndael(blockSize, keySize, irreduciblePolynomial, key);
            CipherMode = cipherMode;
            this.IV = IV;
        }

        #endregion

        #region Properties

        private Rijndael Rijndael
        {
            get =>
                _rijndael ?? throw new ArgumentNullException("Rijndael");

            set =>
                _rijndael = value;
        }

        private SymmetricCipherModes CipherMode
        {
            get =>
                _cipherMode;

            set
            {
                if (!Enum.IsDefined(typeof(SymmetricCipherModes), value))
                {
                    throw new ArgumentException("Invalid cipher mode.", nameof(value));
                }
                _cipherMode = value;
                if (CipherMode == SymmetricCipherModes.ElectronicCodeBook)
                {
                    IV = null;
                }
            }
        }

        private byte[] IV
        {
            get =>
                _IV;

            set
            {
                if (CipherMode == SymmetricCipherModes.ElectronicCodeBook)
                {
                    _IV = null;
                    return;
                }
                if (CipherMode != SymmetricCipherModes.ElectronicCodeBook && value == null)
                {
                    throw new ArgumentNullException(nameof(IV));
                }
                if (value.Length != (int)Rijndael.BlockSize)
                {
                    throw new ArgumentException("IV size should be equal to block size.", nameof(value));
                }
                _IV = (byte[])value.Clone();
            }
        }

        #endregion

        #region Public methods

        public byte[] Encrypt(byte[] dataToEncrypt)
        {
            if (CipherMode == SymmetricCipherModes.ElectronicCodeBook)
            {
                return Rijndael.Encrypt(dataToEncrypt);
            }
            lock (IV)
            {
                switch (CipherMode)
                {
                    case SymmetricCipherModes.CipherBlockChaining:
                        for (var i = 0; i < dataToEncrypt.Length; i++)
                        {
                            dataToEncrypt[i] ^= IV[i];
                        }
                        IV = Rijndael.Encrypt(dataToEncrypt);
                        return dataToEncrypt;
                    case SymmetricCipherModes.CipherFeedback:
                        Rijndael.Encrypt(IV);
                        for (var i = 0; i < IV.Length; i++)
                        {
                            dataToEncrypt[i] ^= IV[i];
                        }
                        IV = dataToEncrypt;
                        return dataToEncrypt;
                    case SymmetricCipherModes.OutputFeedback:
                        Rijndael.Encrypt(IV);
                        for (var i = 0; i < IV.Length; i++)
                        {
                            dataToEncrypt[i] ^= IV[i];
                        }
                        return dataToEncrypt;
                    default:
                        throw new ArgumentException("Invalid cipher mode.", nameof(CipherMode));
                }
            }
        }

        public byte[] Decrypt(byte[] dataToDecrypt)
        {
            if (CipherMode == SymmetricCipherModes.ElectronicCodeBook)
            {
                return Rijndael.Decrypt(dataToDecrypt);
            }
            lock (IV)
            {
                var newIV = default(byte[]);
                switch (CipherMode)
                {
                    case SymmetricCipherModes.CipherBlockChaining:
                        newIV = (byte[])dataToDecrypt.Clone();
                        Rijndael.Decrypt(dataToDecrypt);
                        for (var i = 0; i < dataToDecrypt.Length; i++)
                        {
                            dataToDecrypt[i] ^= IV[i];
                        }
                        IV = newIV;
                        return dataToDecrypt;
                    case SymmetricCipherModes.CipherFeedback:
                        Rijndael.Encrypt(IV);
                        newIV = (byte[])dataToDecrypt.Clone();
                        for (var i = 0; i < IV.Length; i++)
                        {
                            dataToDecrypt[i] ^= IV[i];
                        }
                        IV = newIV;
                        return dataToDecrypt;
                    case SymmetricCipherModes.OutputFeedback:
                        Rijndael.Encrypt(IV);
                        for (var i = 0; i < IV.Length; i++)
                        {
                            dataToDecrypt[i] ^= IV[i];
                        }
                        return dataToDecrypt;
                    default:
                        throw new ArgumentException("Invalid cipher mode.", nameof(CipherMode));
                }
            }
        }

        #endregion

    }

}
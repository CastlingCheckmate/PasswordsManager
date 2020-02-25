using PasswordsManager.Cryptography;

namespace PasswordsManager.UI.Auxiliary.Converters
{

    public static class ConvertersHost
    {

        static ConvertersHost()
        {
            ECBCipherModeToFalse = new SymmetricCipherModesToBoolConverter(SymmetricCipherModes.ElectronicCodeBook);
        }

        public static SymmetricCipherModesToBoolConverter ECBCipherModeToFalse
        {
            get;

            private set;
        }

    }

}
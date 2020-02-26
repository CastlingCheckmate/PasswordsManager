using PasswordsManager.Cryptography;

namespace WPF.UI.Converters
{

    public static class ConvertersHost
    {

        static ConvertersHost()
        {
            ECBCipherModeToFalse = new SymmetricCipherModesToBoolConverter(SymmetricCipherModes.ElectronicCodeBook);
            PasswordMask = new PasswordMaskConverter('*');
        }

        public static SymmetricCipherModesToBoolConverter ECBCipherModeToFalse
        {
            get;

            private set;
        }

        public static PasswordMaskConverter PasswordMask
        {
            get;

            private set;
        }

    }

}
using System;
using System.Globalization;
using System.Windows.Data;

using PasswordsManager.Cryptography;

namespace WPF.UI.Converters
{

    public sealed class SymmetricCipherModesToBoolConverter : IValueConverter
    {

        private readonly SymmetricCipherModes _falseValue;

        public SymmetricCipherModesToBoolConverter(SymmetricCipherModes falseValue)
        {
            _falseValue = falseValue;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SymmetricCipherModes && (SymmetricCipherModes)value == _falseValue)
            {
                return false;
            }
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }

}
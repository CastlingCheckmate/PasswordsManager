using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace WPF.UI.Converters
{

    public sealed class PasswordMaskConverter : IValueConverter
    {

        private char _passwordChar;

        public PasswordMaskConverter(char passwordChar)
        {
            _passwordChar = passwordChar;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is bool && (bool)parameter)
            {
                return value;
            }
            return string.Concat(Enumerable.Repeat(_passwordChar, ((string)value).Length));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
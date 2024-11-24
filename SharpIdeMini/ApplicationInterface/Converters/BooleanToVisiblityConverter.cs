using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SharpIdeMini.ApplicationInterface.Converters
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BooleanToVisiblityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not bool boolean)
                throw new InvalidOperationException("The value must be a boolean");

            return boolean ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}

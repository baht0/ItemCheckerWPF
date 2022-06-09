using System;
using System.Globalization;
using System.Windows.Data;

namespace ItemChecker.Converter
{
    internal class IntToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)value != 0 ? "Visible" : "Hidden";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)value != 0 ? "Visible" : "Hidden";
        }
    }
}

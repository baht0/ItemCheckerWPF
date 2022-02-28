using System;
using System.Globalization;
using System.Windows.Data;

namespace ItemChecker.Converter
{
    internal class BooleanToPlayButtonConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? "Stop" : "Play";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? "Stop" : "Play";
        }
    }
}

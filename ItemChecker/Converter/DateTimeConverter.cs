using System;
using System.Globalization;
using System.Windows.Data;

namespace ItemChecker.Converter
{
    internal class DateTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (DateTime)value == new DateTime() ? "Free" : $"{Math.Floor((((DateTime)value) - DateTime.Now).TotalDays)} Days";
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (DateTime)value == new DateTime() ? "Free" : $"{Math.Floor((((DateTime)value) - DateTime.Now).TotalDays)} Days";
        }
    }
}

using System;
using System.Globalization;
using System.Windows.Data;

namespace ItemChecker.Converter
{
    internal class TradeLockConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int days = (int)Math.Floor((((DateTime)value) - DateTime.Now).TotalDays);
            days = days > 0 ? days : days * (-1);
            return (DateTime)value == new DateTime() ? "Free" : $"{ToReadable(value)} | {days} Days";
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int days = (int)Math.Floor((((DateTime)value) - DateTime.Now).TotalDays);
            days = days > 0 ? days : days * (-1);
            return (DateTime)value == new DateTime() ? "Free" : $"{ToReadable(value)} | {days} Days";
        }

        private static string ToReadable(object value) => ((DateTime)value).ToString("dd MMM yy");
    }
}

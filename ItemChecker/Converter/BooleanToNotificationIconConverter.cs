using System;
using System.Globalization;
using System.Windows.Data;

namespace ItemChecker.Converter
{
    public class BooleanToNotificationIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolean)
            {
                if (boolean)
                    return "BellBadge";
                else
                    return "Bell";
            }
            return "Bell";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolean)
            {
                if (boolean)
                    return "BellBadge";
                else
                    return "Bell";
            }
            return "Bell";
        }
    }
}

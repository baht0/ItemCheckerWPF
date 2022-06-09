using ItemChecker.MVVM.Model;
using System;
using System.Globalization;
using System.Windows.Data;

namespace ItemChecker.Converter
{
    internal class IntToRareParameterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            RareCheckConfig rareCheck = new();
            return rareCheck.Parameters[(int)value];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            RareCheckConfig rareCheck = new();
            return rareCheck.Parameters[(int)value];
        }
    }
}

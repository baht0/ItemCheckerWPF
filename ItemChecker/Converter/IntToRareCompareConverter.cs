using ItemChecker.MVVM.Model;
using System;
using System.Globalization;
using System.Windows.Data;

namespace ItemChecker.Converter
{
    internal class IntToRareCompareConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            RareCheckConfig rareCheck = new();
            return rareCheck.ComparePrices[(int)value];

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            RareCheckConfig rareCheck = new();
            return rareCheck.ComparePrices[(int)value];
        }
    }
}

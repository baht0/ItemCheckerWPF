using ItemChecker.MVVM.Model;
using System.Globalization;
using System.Windows.Data;

namespace ItemChecker.Converter
{
    internal class IntToRareCompareConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            RareConfig rareCheck = new();
            return rareCheck.ComparePrices[(int)value];
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            RareConfig rareCheck = new();
            return rareCheck.ComparePrices[(int)value];
        }
    }
}

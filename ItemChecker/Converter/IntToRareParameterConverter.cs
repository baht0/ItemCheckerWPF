using ItemChecker.MVVM.Model;
using System.Globalization;
using System.Windows.Data;

namespace ItemChecker.Converter
{
    internal class IntToRareParameterConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            return BaseModel.Parameters[(int)value];
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            return BaseModel.Parameters[(int)value];
        }
    }
}

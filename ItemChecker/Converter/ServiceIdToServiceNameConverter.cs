using ItemChecker.MVVM.Model;
using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace ItemChecker.Converter
{
    internal class ServiceIdToServiceNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null)
                return Main.Services[(int)value];

            var combobox = (ComboBox)parameter;
            var services = combobox.Items;
            return services[(int)value];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null)
                return Main.Services[(int)value];

            var combobox = (ComboBox)parameter;
            var services = combobox.Items;
            return services[(int)value];
        }
    }
}

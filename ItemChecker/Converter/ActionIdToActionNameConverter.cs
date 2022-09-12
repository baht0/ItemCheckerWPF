using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace ItemChecker.Converter
{
    class ActionIdToActionNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ComboBox combobox = (ComboBox)parameter;
            var services = combobox.Items;
            return services[(int)value];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ComboBox combobox = (ComboBox)parameter;
            var services = combobox.Items;
            return services[(int)value];
        }
    }
}

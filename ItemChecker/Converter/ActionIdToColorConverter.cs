using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace ItemChecker.Converter
{
    class ActionIdToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ComboBox combobox = (ComboBox)parameter;
            var services = combobox.Items;
            switch (services[(int)value])
            {
                case "Withdraw":
                    return "#fa6b6b";
                case "Deposit":
                    return "#63bf60";
            }
            return "Red";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ComboBox combobox = (ComboBox)parameter;
            var services = combobox.Items;
            switch (services[(int)value])
            {
                case "Withdraw":
                    return "#fa6b6b";
                case "Deposit":
                    return "#63bf60";
            }
            return "Red";
        }
    }
}

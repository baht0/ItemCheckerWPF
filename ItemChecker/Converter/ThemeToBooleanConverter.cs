﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace ItemChecker.Converter
{
    class ThemeToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (string)value == "WeatherNight";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value;
        }
    }
}

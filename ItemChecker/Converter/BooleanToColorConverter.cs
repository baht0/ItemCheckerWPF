﻿using ItemChecker.MVVM.Model;
using System;
using System.Globalization;
using System.Windows.Data;

namespace ItemChecker.Converter
{
    public class BooleanToColorConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            if (!(bool)value)
            {
                switch (ProjectInfo.Theme)
                {
                    case "Light":
                        return "Black";
                    case "Dark":
                        return "WhiteSmoke";
                }
            }
            return "Gray";
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            if (!(bool)value)
            {
                switch (ProjectInfo.Theme)
                {
                    case "Light":
                        return "Black";
                    case "Dark":
                        return "WhiteSmoke";
                }
            }
            return "Gray";
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace PLSE_FoxPro.Converters
{
    class BoolToGenderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value switch
        {
            bool b when b == true => "женский",
            bool b when b == false => "мужской",
            _ => "неизвестный",
        };
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value switch
        {
            string s when s.Equals("мужской", StringComparison.OrdinalIgnoreCase) => false,
            string s when s.Equals("женский", StringComparison.OrdinalIgnoreCase) => true,
            _ => null,
        };
    }
}

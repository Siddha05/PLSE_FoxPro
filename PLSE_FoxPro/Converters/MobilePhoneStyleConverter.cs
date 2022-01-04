using System;
using System.Globalization;
using System.Windows.Data;
using PLSE_FoxPro.Models;

namespace PLSE_FoxPro.Converters
{
    internal class MobilePhoneStyleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value switch
        {
            string s when s.Length != 10 => s,
            string s => s.FormatMobilePhone(),
            _ => value 
        };

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => (value as string).OnlyDigits();
    }
}

using System;
using System.Globalization;
using System.Windows.Data;
using PLSE_FoxPro.Models;

namespace PLSE_FoxPro.Converters
{
    internal class WorkPhoneStyleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            value switch
            {
                string s when s.Length < 4 || s.Length > 7 => s,
                string s => s.FormatWorkPhone(),
                _ => value
            };
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => (value as string).OnlyDigits();
    }
}

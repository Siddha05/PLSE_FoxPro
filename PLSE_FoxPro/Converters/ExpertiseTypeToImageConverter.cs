using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace PLSE_FoxPro.Converters
{
    internal class ExpertiseTypeToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value switch
        {
            string s when s.Equals("первичная") => "RayStartArrow",
            string s when s.Equals("дополнительная") => "Plus",
            string s when s.Equals("повторная") => "Restart",
            _ => "Help"
        };

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

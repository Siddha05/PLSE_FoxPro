using PLSE_FoxPro.Models;
using System;
using System.Globalization;
using System.Windows.Data;

namespace PLSE_FoxPro.Converters
{
    public class TimeAgoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var d = value as DateTime?;
            return d.TimeAgo();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

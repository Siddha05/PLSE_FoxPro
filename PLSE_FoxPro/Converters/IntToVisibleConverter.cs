using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PLSE_FoxPro.Converters 
{
    [ValueConversion(typeof(int), typeof(Visibility))]
    public class IntToVisibleConverter : IValueConverter
    {
        public int Threshold { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility p = Visibility.Visible;
            Enum.TryParse(parameter?.ToString(), out p);
            if ((int)value <= Threshold)
            {
                return p;
            }
            else return p == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

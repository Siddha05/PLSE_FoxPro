using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace PLSE_FoxPro.Converters
{
    internal class BillPriceConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            byte hours = 0;
            decimal hp = 0;
            if (values[0] != DependencyProperty.UnsetValue && values[0] != null)
            {
                hours = (byte)values[0];
            }
            if (values[1] != DependencyProperty.UnsetValue && values[1] != null)
            {
                hp = (decimal)values[1];
            }
            return hours * hp;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

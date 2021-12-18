using PLSE_FoxPro.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace PLSE_FoxPro.Converters
{
    public class MessageToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var c = (MessageType)value;
            switch (c)
            {
                case MessageType.Success:
                    return "Checkbox";
                case MessageType.Warning:
                    return "AlertOctagramOutline";
                case MessageType.Error:
                    return "AlertOutline";
                case MessageType.Congratulation:
                    return "Gift";
                default:
                    return "MessageTextOutline";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// Конвертер перевода типа сообщения в цвет, описываемый строкой
    /// </summary>
    public class MessageToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((MessageType)value)
            {
                case MessageType.Success:
                    return "#FF0BDA0B";
                case MessageType.Warning:
                    return "#FFEAEA09";
                case MessageType.Error:
                    return "Red";
                case MessageType.Congratulation:
                    return "GreenYellow";
                default:
                    return "#FF4A03C3";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

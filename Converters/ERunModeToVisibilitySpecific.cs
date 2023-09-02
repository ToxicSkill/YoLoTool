using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using YoLoTool.Enums;

namespace YoLoTool.Converters
{
    public class ERunModeToVisibilitySpecific : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ERunMode runMode)
            {
                return runMode == ERunMode.Prelabeling ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

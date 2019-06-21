using System;
using System.Globalization;
using System.Windows.Data;

namespace CHAD.DesktopApplication.Converters
{
    public class FinishEnabledConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if(values.Length < 2)
                throw new ArgumentException(nameof(values));

            var navigationServiceNextEnabled = (bool)values[0];
            var name = (string) values[1];

            return navigationServiceNextEnabled && !string.IsNullOrEmpty(name);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

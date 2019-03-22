using System;
using System.Globalization;
using System.Windows.Data;

namespace CHAD.DesktopApplication.Converters
{
    public class NullToBoolConverter : IValueConverter
    {
        #region Interface Implementations

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
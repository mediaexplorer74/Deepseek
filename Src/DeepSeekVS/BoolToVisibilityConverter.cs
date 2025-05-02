/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepSeekVS
{
    internal class BoolToVisibilityConverter
    {
    }
}*/
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DeepSeekVS
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                if (parameter is string param && param.Equals("Invert", StringComparison.OrdinalIgnoreCase))
                {
                    boolValue = !boolValue;
                }

                return boolValue ? Visibility.Visible :
                    (parameter?.ToString() == "Hidden" ? Visibility.Hidden : Visibility.Collapsed);
            }
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                return visibility == Visibility.Visible;
            }
            return DependencyProperty.UnsetValue;
        }
    }
}

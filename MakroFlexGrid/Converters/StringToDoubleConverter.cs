using System;
using System.Globalization;
using System.Windows.Data;

namespace MakroFlexGrid.Converters
{
    /// <summary>
    /// Конвертирует строку в double для ProgressBar.
    /// </summary>
    [ValueConversion(typeof(string), typeof(double))]
    public class StringToDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double)
                return (double)value;
            if (value is int)
                return (double)(int)value;
            if (value is decimal)
                return (double)(decimal)value;
            if (value is string s && !string.IsNullOrEmpty(s))
            {
                if (double.TryParse(s, NumberStyles.Any, culture, out double result))
                    return result;
            }
            return 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
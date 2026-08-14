using System;
using System.Globalization;
using System.Windows.Data;

namespace MakroFlexGrid.Converters
{
    /// <summary>
    /// Конвертирует строку "true"/"false" в bool? для CheckBox.
    /// Поддерживает bool, bool?, string.
    /// </summary>
    [ValueConversion(typeof(object), typeof(bool?))]
    public class StringToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
                return (bool)value;
            if (value is string s)
            {
                if (bool.TryParse(s, out bool result))
                    return result;
                if (s == "1" || s.ToLower() == "yes" || s.ToLower() == "да")
                    return true;
                if (s == "0" || s.ToLower() == "no" || s.ToLower() == "нет")
                    return false;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
                return ((bool)value).ToString();
            return "false";
        }
    }
}
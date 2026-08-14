using System;
using System.Globalization;
using System.Windows.Data;

namespace MakroFlexGrid.Converters
{
    /// <summary>
    /// Конвертирует строку в DateTime? и обратно.
    /// Используется для DatePicker в DateCell.
    /// </summary>
    [ValueConversion(typeof(string), typeof(DateTime?))]
    public class StringToDateTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime)
                return (DateTime)value;
            if (value is string s && !string.IsNullOrEmpty(s))
            {
                if (DateTime.TryParse(s, culture, DateTimeStyles.None, out DateTime result))
                    return result;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime)
                return ((DateTime)value).ToString(culture);
            return "";
        }
    }
}
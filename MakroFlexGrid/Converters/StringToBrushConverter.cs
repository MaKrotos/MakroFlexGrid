using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace MakroFlexGrid.Converters
{
    /// <summary>
    /// Конвертирует название цвета или HEX-строку в Brush.
    /// Поддерживает "Red", "#FF0000", "255,0,0" и т.д.
    /// Используется для ColorCell.
    /// </summary>
    [ValueConversion(typeof(string), typeof(Brush))]
    public class StringToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Brush brush)
                return brush;
            if (value is string s && !string.IsNullOrEmpty(s))
            {
                try
                {
                    var color = (Color)ColorConverter.ConvertFromString(s);
                    return new SolidColorBrush(color);
                }
                catch
                {
                    // Если не удалось распарсить, возвращаем серый
                }
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SolidColorBrush brush)
                return brush.Color.ToString();
            return "";
        }
    }
}
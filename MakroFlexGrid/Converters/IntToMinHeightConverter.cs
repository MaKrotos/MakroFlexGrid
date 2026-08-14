using System;
using System.Globalization;
using System.Windows.Data;

namespace MakroFlexGrid.Converters
{
    /// <summary>
    /// Конвертирует int (количество строк) в double (минимальную высоту) для MultiLineCell.
    /// </summary>
    [ValueConversion(typeof(int), typeof(double))]
    public class IntToMinHeightConverter : IValueConverter
    {
        /// <summary>
        /// Высота одной строки текста в пикселях.
        /// </summary>
        public double LineHeight { get; set; } = 20.0;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int lines && lines > 0)
                return lines * LineHeight;
            return 100.0; // default
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
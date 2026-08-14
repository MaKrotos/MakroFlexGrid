using System.Globalization;
using System.Windows.Data;

namespace MakroFlexGrid.Converters
{
    /// <summary>
    /// Конвертер для сравнения типа объекта с указанным типом.
    /// Использует IsInstanceOfType, что корректно обрабатывает наследование.
    /// </summary>
    public class TypeMatchConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            // parameter должен быть Type
            if (parameter is Type targetTypeToMatch)
                return targetTypeToMatch.IsInstanceOfType(value);

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
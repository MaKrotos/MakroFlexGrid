using System;
using System.Globalization;
using System.Windows.Data;

namespace MakroFlexGrid.Converters
{
    /// <summary>
    /// Сравнивает значение с параметром конвертера.
    /// Возвращает true, если равны.
    /// Используется для RadioButtonCell — определяет, выбран ли данный вариант.
    /// </summary>
    [ValueConversion(typeof(object), typeof(bool))]
    public class EqualityConverter : IValueConverter, IMultiValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null && parameter == null)
                return true;
            if (value == null || parameter == null)
                return false;
            return value.Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isChecked && isChecked)
                return parameter;
            return Binding.DoNothing;
        }

        // IMultiValueConverter: первый аргумент — значение, второй — параметр (опция)
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2)
                return false;
            var val = values[0];
            var option = values[1];
            if (val == null && option == null)
                return true;
            if (val == null || option == null)
                return false;
            return val.Equals(option);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
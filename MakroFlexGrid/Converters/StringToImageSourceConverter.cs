using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MakroFlexGrid.Converters
{
    /// <summary>
    /// Конвертирует строку (URI, base64, путь к ресурсу) в ImageSource.
    /// Используется для ImageCell.
    /// </summary>
    [ValueConversion(typeof(string), typeof(ImageSource))]
    public class StringToImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ImageSource imageSource)
                return imageSource;
            if (value is string s && !string.IsNullOrEmpty(s))
            {
                try
                {
                    // Пробуем как URI
                    if (Uri.TryCreate(s, UriKind.RelativeOrAbsolute, out Uri uri))
                    {
                        return new BitmapImage(uri);
                    }
                }
                catch
                {
                    // Если не получилось, возвращаем null
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
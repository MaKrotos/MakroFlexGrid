using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MakroFlexGrid.Converters
{
    public class RightFrozenVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int rightFrozenCount = value is int count ? count : 0;
            return rightFrozenCount > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DoubleToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double val = value is double d ? d : 0;
            return val > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is String str)
            {
                if (!String.IsNullOrEmpty(str))
                {
                    return Visibility.Visible;
                }
                else return Visibility.Collapsed;
            }

            bool val = value is bool b && b;
            if (parameter as string == "Invert")
            {
                return !val ? Visibility.Visible : Visibility.Collapsed;
            }
            return val ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class VisibilityConverter : BoolToVisibilityConverter
    {
    }
}

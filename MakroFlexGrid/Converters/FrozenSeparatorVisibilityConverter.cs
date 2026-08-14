using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MakroFlexGrid.Converters
{
    /// <summary>
    /// Multi-value converter that returns Visibility.Visible only when
    /// SeparatorWidth > 0 AND FrozenColumnsCount > 0.
    /// Used to hide separators when the corresponding frozen panel is empty.
    /// 
    /// Parameters:
    ///   values[0] = SeparatorWidth (double)
    ///   values[1] = FrozenColumnsCount (int)
    /// </summary>
    public class FrozenSeparatorVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double separatorWidth = values.Length > 0 && values[0] is double d ? d : 0;
            int frozenCount = values.Length > 1 && values[1] is int count ? count : 0;

            return separatorWidth > 0 && frozenCount > 0
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
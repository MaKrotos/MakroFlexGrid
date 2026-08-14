using MakroFlexGrid.Rows;
using System.Globalization;
using System.Windows.Data;

namespace MakroFlexGrid.Converters
{
    public class PanelTextPositionToRowConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int row = 0;
            if (value is BottomPanelViewModel.PanelTextPosition position)
            {
                row = position == BottomPanelViewModel.PanelTextPosition.Top ? 0 : 1;
            }

            if (parameter != null && parameter.ToString() == "Invert")
            {
                row = row == 0 ? 1 : 0;
            }

            return row;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
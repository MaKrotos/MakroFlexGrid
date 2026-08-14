using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MakroFlexGrid.Converters
{
    /// <summary>
    /// Конвертирует bool в Thickness для левого бордюра ячейки.
    /// true -> "1,0,1,1" (с левым бордюром) — для основных строк
    /// false -> "0,0,1,1" (без левого бордюра) — для основных строк
    /// 
    /// С параметром "Bottom":
    /// true -> "1,0,1,0" (с левым бордюром) — для нижней панели
    /// false -> "0,0,1,0" (без левого бордюра) — для нижней панели
    /// 
    /// Используется для первой (самой левой) ячейки правой frozen-панели,
    /// чтобы визуально отделить её от центральной скроллируемой панели.
    /// </summary>
    public class BoolToLeftBorderConverter : IValueConverter
    {
        private static readonly Thickness WithLeftBorder = new Thickness(1, 0, 1, 1);
        private static readonly Thickness WithoutLeftBorder = new Thickness(0, 0, 1, 1);
        private static readonly Thickness WithLeftBorderBottom = new Thickness(1, 0, 1, 0);
        private static readonly Thickness WithoutLeftBorderBottom = new Thickness(0, 0, 1, 0);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isBottom = parameter is string param && param == "Bottom";
            bool isLeftmost = value is bool b && b;

            if (isBottom)
                return isLeftmost ? WithLeftBorderBottom : WithoutLeftBorderBottom;

            return isLeftmost ? WithLeftBorder : WithoutLeftBorder;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

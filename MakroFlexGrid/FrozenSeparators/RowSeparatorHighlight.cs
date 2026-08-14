using System.Windows;

namespace MakroFlexGrid.FrozenSeparators
{
    /// <summary>
    /// Attached properties для синхронизации подсветки разделителей (RowSeparatorGripper)
    /// между всеми строками и шапкой таблицы.
    /// Когда пользователь наводит или захватывает левый гриппер,
    /// подсвечиваются все левые грипперы и левый разделитель в шапке.
    /// Аналогично для правого гриппера.
    /// </summary>
    public static class RowSeparatorHighlight
    {
        public static readonly DependencyProperty IsLeftHighlightedProperty =
            DependencyProperty.RegisterAttached(
                "IsLeftHighlighted",
                typeof(bool),
                typeof(RowSeparatorHighlight),
                new PropertyMetadata(false));

        public static bool GetIsLeftHighlighted(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsLeftHighlightedProperty);
        }

        public static void SetIsLeftHighlighted(DependencyObject obj, bool value)
        {
            obj.SetValue(IsLeftHighlightedProperty, value);
        }

        public static readonly DependencyProperty IsRightHighlightedProperty =
            DependencyProperty.RegisterAttached(
                "IsRightHighlighted",
                typeof(bool),
                typeof(RowSeparatorHighlight),
                new PropertyMetadata(false));

        public static bool GetIsRightHighlighted(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsRightHighlightedProperty);
        }

        public static void SetIsRightHighlighted(DependencyObject obj, bool value)
        {
            obj.SetValue(IsRightHighlightedProperty, value);
        }
    }
}

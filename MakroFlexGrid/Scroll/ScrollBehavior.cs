using System.Windows;
using System.Windows.Media;

namespace MakroFlexGrid.Scroll
{
    /// <summary>
    /// Attached behavior for applying horizontal scroll offset to a StackPanel
    /// via TranslateTransform on the panel itself, rather than on the parent container.
    /// This prevents the container from being shifted and clipped.
    /// </summary>
    public static class ScrollBehavior
    {
        public static readonly DependencyProperty HorizontalOffsetProperty =
            DependencyProperty.RegisterAttached(
                "HorizontalOffset",
                typeof(double),
                typeof(ScrollBehavior),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender, OnHorizontalOffsetChanged));

        public static double GetHorizontalOffset(DependencyObject obj)
        {
            return (double)obj.GetValue(HorizontalOffsetProperty);
        }

        public static void SetHorizontalOffset(DependencyObject obj, double value)
        {
            obj.SetValue(HorizontalOffsetProperty, value);
        }

        private static void OnHorizontalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element)
            {
                double offset = (double)e.NewValue;

                if (element.RenderTransform is TranslateTransform transform)
                {
                    transform.X = offset;
                }
                else
                {
                    element.RenderTransform = new TranslateTransform(offset, 0);
                }
            }
        }
    }
}

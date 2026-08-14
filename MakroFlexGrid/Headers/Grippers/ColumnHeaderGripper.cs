using System.Windows;
using System.Windows.Controls.Primitives;

namespace MakroFlexGrid.Headers
{
    /// <summary>
    /// Thumb для изменения ширины колонки (ресайз).
    /// Аналог BandHeaderGripper из FlexGrid.
    /// </summary>
    public sealed class ColumnHeaderGripper : Thumb
    {
        static ColumnHeaderGripper()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ColumnHeaderGripper),
                new FrameworkPropertyMetadata(typeof(ColumnHeaderGripper)));
        }
    }
}

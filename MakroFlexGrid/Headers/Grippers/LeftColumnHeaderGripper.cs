using System.Windows;
using System.Windows.Controls.Primitives;

namespace MakroFlexGrid.Headers
{
    /// <summary>
    /// Thumb для изменения ширины колонки с левого края.
    /// </summary>
    public sealed class LeftColumnHeaderGripper : Thumb
    {
        static LeftColumnHeaderGripper()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(LeftColumnHeaderGripper),
                new FrameworkPropertyMetadata(typeof(LeftColumnHeaderGripper)));
        }
    }
}

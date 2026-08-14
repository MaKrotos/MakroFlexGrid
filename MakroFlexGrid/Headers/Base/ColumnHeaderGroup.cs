using System.Windows;
using System.Windows.Markup;

namespace MakroFlexGrid.Headers
{
    /// <summary>
    /// Группа колонок — элемент заголовка, который может содержать дочерние подзаголовки.
    /// Используется для создания многоуровневых заголовков.
    /// </summary>
    [ContentProperty("Children")]
    public sealed class ColumnHeaderGroup : ColumnHeaderItem
    {
        static ColumnHeaderGroup()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ColumnHeaderGroup),
                new FrameworkPropertyMetadata(typeof(ColumnHeaderGroup)));
        }

        public ColumnHeaderGroup()
        {
            // Группа по умолчанию не участвует в сортировке
            CanUserSort = false;
        }
    }
}

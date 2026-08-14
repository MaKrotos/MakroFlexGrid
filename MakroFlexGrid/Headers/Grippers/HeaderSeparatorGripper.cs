using MakroFlexGrid.Core;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace MakroFlexGrid.Headers
{
    /// <summary>
    /// Thumb для изменения ширины замороженных панелей (frozen columns) в заголовках.
    /// Используется для PART_LeftHeaderSeparator и PART_RightHeaderSeparator.
    /// </summary>
    public sealed class HeaderSeparatorGripper : Thumb
    {
        /// <summary>
        /// Определяет, с какой стороны находится гриппер: false = левый (между left frozen и center),
        /// true = правый (между center и right frozen).
        /// </summary>
        public static readonly DependencyProperty IsRightProperty =
            DependencyProperty.Register(nameof(IsRight), typeof(bool),
            typeof(HeaderSeparatorGripper), new PropertyMetadata(false));

        public bool IsRight
        {
            get => (bool)GetValue(IsRightProperty);
            set => SetValue(IsRightProperty, value);
        }

        static HeaderSeparatorGripper()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(HeaderSeparatorGripper),
                new FrameworkPropertyMetadata(typeof(HeaderSeparatorGripper)));
        }

        public HeaderSeparatorGripper()
        {
            DragDelta += OnDragDelta;
            DragStarted += OnDragStarted;
            DragCompleted += OnDragCompleted;
            MouseEnter += OnMouseEnter;
            MouseLeave += OnMouseLeave;
        }

        private CustomDataGrid FindGrid()
        {
            return FindParent<CustomDataGrid>(this);
        }

        private void OnMouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var grid = FindGrid();
            if (grid != null)
                SetHighlight(grid, true);
        }

        private void OnMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var grid = FindGrid();
            if (grid != null && !IsDragging)
                SetHighlight(grid, false);
        }

        private void OnDragStarted(object sender, DragStartedEventArgs e)
        {
            var grid = FindGrid();
            if (grid != null)
                SetHighlight(grid, true);
        }

        private void OnDragCompleted(object sender, DragCompletedEventArgs e)
        {
            var grid = FindGrid();
            if (grid != null)
                SetHighlight(grid, false);
        }

        private void SetHighlight(CustomDataGrid grid, bool value)
        {
            if (IsRight)
                MakroFlexGrid.FrozenSeparators.RowSeparatorHighlight.SetIsRightHighlighted(grid, value);
            else
                MakroFlexGrid.FrozenSeparators.RowSeparatorHighlight.SetIsLeftHighlighted(grid, value);
        }

        private void OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            var grid = FindGrid();
            if (grid == null) return;

            if (IsRight)
            {
                ResizeRightFrozenColumn(grid, e.HorizontalChange);
            }
            else
            {
                ResizeLeftFrozenColumn(grid, e.HorizontalChange);
            }
        }

        private static void ResizeLeftFrozenColumn(CustomDataGrid grid, double horizontalChange)
        {
            int leftCount = grid.LeftFrozenColumnsCount;
            if (leftCount <= 0) return;

            var rootItems = new List<ColumnHeaderItem>();
            for (int i = 0; i < leftCount; i++)
            {
                var column = grid.Columns[i];
                if (column == null) continue;

                var headerItem = grid.GetColumnHeaderItem(column);
                var rootItem = headerItem?.GetRootItem();
                if (rootItem == null) continue;

                if (!rootItems.Contains(rootItem))
                    rootItems.Add(rootItem);
            }

            if (rootItems.Count == 0) return;

            double totalWidth = 0;
            foreach (var item in rootItems)
                totalWidth += item.Width;

            if (totalWidth <= 0) return;

            double newTotalWidth = totalWidth + horizontalChange;
            double adjustedTotalWidth = newTotalWidth;
            foreach (var item in rootItems)
            {
                double proportion = item.Width / totalWidth;
                double newItemWidth = newTotalWidth * proportion;

                if (newItemWidth < item.MinWidth)
                {
                    double deficit = item.MinWidth - newItemWidth;
                    adjustedTotalWidth = Math.Max(adjustedTotalWidth, newTotalWidth + deficit);
                }

                if (!double.IsNaN(item.MaxWidth) && newItemWidth > item.MaxWidth)
                {
                    double excess = newItemWidth - item.MaxWidth;
                    adjustedTotalWidth = Math.Min(adjustedTotalWidth, newTotalWidth - excess);
                }
            }

            foreach (var root in rootItems)
            {
                double proportion = root.Width / totalWidth;
                double finalWidth = adjustedTotalWidth * proportion;

                finalWidth = Math.Max(root.MinWidth, finalWidth);
                if (!double.IsNaN(root.MaxWidth))
                    finalWidth = Math.Min(finalWidth, root.MaxWidth);

                root.Width = finalWidth;
            }
        }

        private static void ResizeRightFrozenColumn(CustomDataGrid grid, double horizontalChange)
        {
            int rightCount = grid.RightFrozenColumnsCount;
            if (rightCount <= 0) return;

            int totalColumns = grid.Columns.Count;
            var rootItems = new List<ColumnHeaderItem>();
            for (int i = totalColumns - rightCount; i < totalColumns; i++)
            {
                var column = grid.Columns[i];
                if (column == null) continue;

                var headerItem = grid.GetColumnHeaderItem(column);
                var rootItem = headerItem?.GetRootItem();
                if (rootItem == null) continue;

                if (!rootItems.Contains(rootItem))
                    rootItems.Add(rootItem);
            }

            if (rootItems.Count == 0) return;

            double totalWidth = 0;
            foreach (var item in rootItems)
                totalWidth += item.Width;

            if (totalWidth <= 0) return;

            double newTotalWidth = totalWidth - horizontalChange;
            double adjustedTotalWidth = newTotalWidth;
            foreach (var item in rootItems)
            {
                double proportion = item.Width / totalWidth;
                double newItemWidth = newTotalWidth * proportion;

                if (newItemWidth < item.MinWidth)
                {
                    double deficit = item.MinWidth - newItemWidth;
                    adjustedTotalWidth = Math.Max(adjustedTotalWidth, newTotalWidth + deficit);
                }

                if (!double.IsNaN(item.MaxWidth) && newItemWidth > item.MaxWidth)
                {
                    double excess = newItemWidth - item.MaxWidth;
                    adjustedTotalWidth = Math.Min(adjustedTotalWidth, newTotalWidth - excess);
                }
            }

            foreach (var item in rootItems)
            {
                double proportion = item.Width / totalWidth;
                double newItemWidth = adjustedTotalWidth * proportion;

                newItemWidth = Math.Max(item.MinWidth, newItemWidth);
                if (!double.IsNaN(item.MaxWidth))
                    newItemWidth = Math.Min(newItemWidth, item.MaxWidth);

                item.Width = newItemWidth;
            }
        }

        private static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject current = child;
            while (current != null)
            {
                current = VisualTreeHelper.GetParent(current);
                if (current is T result)
                    return result;
            }
            return null;
        }
    }
}

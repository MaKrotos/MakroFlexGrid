using MakroFlexGrid.Core;
using MakroFlexGrid.Headers;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace MakroFlexGrid.FrozenSeparators
{
    /// <summary>
    /// Thumb для изменения ширины замороженных панелей (frozen columns) в строках.
    /// Аналог ColumnHeaderGripper, но работает на уровне строк и меняет ширину
    /// последней левой frozen-колонки (для левого разделителя) или первой правой
    /// frozen-колонки (для правого разделителя).
    /// При изменении ширины колонки также синхронизирует ColumnHeaderItem,
    /// чтобы заголовки отображались корректно.
    /// </summary>
    public sealed class RowSeparatorGripper : Thumb
    {
        /// <summary>
        /// Определяет, с какой стороны находится гриппер: false = левый (между left frozen и center),
        /// true = правый (между center и right frozen).
        /// </summary>
        public static readonly DependencyProperty IsRightProperty =
            DependencyProperty.Register(nameof(IsRight), typeof(bool),
            typeof(RowSeparatorGripper), new PropertyMetadata(false));

        public bool IsRight
        {
            get => (bool)GetValue(IsRightProperty);
            set => SetValue(IsRightProperty, value);
        }

        static RowSeparatorGripper()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(RowSeparatorGripper),
                new FrameworkPropertyMetadata(typeof(RowSeparatorGripper)));
        }

        public RowSeparatorGripper()
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
                RowSeparatorHighlight.SetIsRightHighlighted(grid, value);
            else
                RowSeparatorHighlight.SetIsLeftHighlighted(grid, value);
        }

        private void OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            var grid = FindGrid();
            if (grid == null)
            {
                System.Diagnostics.Debug.WriteLine($"[RowSeparatorGripper] IsRight={IsRight}: FindGrid() вернул null!");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"[RowSeparatorGripper] IsRight={IsRight}: DragDelta HorizontalChange={e.HorizontalChange}, LeftFrozenColumnsCount={grid.LeftFrozenColumnsCount}, RightFrozenColumnsCount={grid.RightFrozenColumnsCount}");

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

            // Собираем корневые заголовки для каждой из левых frozen-колонок
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

            // Вычисляем общую текущую ширину всех корневых заголовков
            double totalWidth = 0;
            foreach (var item in rootItems)
                totalWidth += item.Width;

            if (totalWidth <= 0) return;

            // Новая общая ширина после применения horizontalChange.
            // При движении гриппера вправо (horizontalChange > 0) ширина должна расти.
            double newTotalWidth = totalWidth + horizontalChange;

            // Проверяем ограничения MinWidth/MaxWidth для каждого корневого заголовка
            // и корректируем newTotalWidth, если какой-то заголовок достиг предела
            double adjustedTotalWidth = newTotalWidth;
            foreach (var item in rootItems)
            {
                double proportion = item.Width / totalWidth;
                double newItemWidth = newTotalWidth * proportion;

                if (newItemWidth < item.MinWidth)
                {
                    // Если новый размер меньше MinWidth, корректируем общую ширину
                    double deficit = item.MinWidth - newItemWidth;
                    adjustedTotalWidth = Math.Max(adjustedTotalWidth, newTotalWidth + deficit);
                }

                if (!double.IsNaN(item.MaxWidth) && newItemWidth > item.MaxWidth)
                {
                    // Если новый размер больше MaxWidth, корректируем общую ширину
                    double excess = newItemWidth - item.MaxWidth;
                    adjustedTotalWidth = Math.Min(adjustedTotalWidth, newTotalWidth - excess);
                }
            }

            // Применяем изменения только к корневым элементам.
            // В архитектуре ColumnHeaderItem изменение ширины корневого элемента
            // должно автоматически приводить к изменению ширины вложенных элементов,
            // если они правильно настроены в UI (например, через Binding или пропорции).
            // Если мы меняем и корень, и детей вручную, мы можем получить двойное изменение.
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

            // Собираем уникальные корневые заголовки для всех правых frozen-колонок
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

            // Вычисляем общую текущую ширину всех корневых заголовков
            double totalWidth = 0;
            foreach (var item in rootItems)
                totalWidth += item.Width;

            if (totalWidth <= 0) return;

            // Для правого разделителя: при перетаскивании влево (horizontalChange < 0)
            // ширина правой панели увеличивается, вправо — уменьшается.
            // Инвертируем horizontalChange.
            double newTotalWidth = totalWidth - horizontalChange;

            // Проверяем ограничения MinWidth/MaxWidth для каждого корневого заголовка
            // и корректируем newTotalWidth, если какой-то заголовок достиг предела
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

            // Применяем скорректированную общую ширину пропорционально
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

        private static void CollectAllHeaderItems(ColumnHeaderItem item, List<ColumnHeaderItem> allItems)
        {
            if (item == null || allItems.Contains(item)) return;
            allItems.Add(item);
            if (item.Children != null)
            {
                foreach (var child in item.Children)
                    CollectAllHeaderItems(child, allItems);
            }
        }
    }
}

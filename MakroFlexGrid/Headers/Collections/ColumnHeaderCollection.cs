using MakroFlexGrid.Core;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace MakroFlexGrid.Headers
{
    /// <summary>
    /// Коллекция элементов заголовков колонок.
    /// Поддерживает вычисление MaxDepth, BottomItemsCount, TotalWidth.
    /// Аналог BandCollection из FlexGrid.
    /// </summary>
    public sealed class ColumnHeaderCollection : ObservableCollection<ColumnHeaderItem>
    {
        /// <summary>
        /// Конструктор по умолчанию для использования из XAML.
        /// OwnerGrid и ParentItem устанавливаются через DependencyProperty callback
        /// или через существующие конструкторы.
        /// </summary>
        public ColumnHeaderCollection()
        {
        }

        internal ColumnHeaderCollection(CustomDataGrid ownerGrid)
        {
            OwnerGrid = ownerGrid;
        }

        internal ColumnHeaderCollection(ColumnHeaderItem parentItem)
        {
            OwnerGrid = parentItem.OwnerGrid;
            ParentItem = parentItem;
        }

        #region Private Variables

        private CustomDataGrid _ownerGrid;

        #endregion

        #region Internal Properties

        internal CustomDataGrid OwnerGrid
        {
            get => _ownerGrid;
            set
            {
                if (_ownerGrid != value)
                {
                    _ownerGrid = value;
                    foreach (var item in this)
                        item.OwnerGrid = value;
                }
            }
        }

        #endregion

        #region Public Properties

        public ColumnHeaderItem ParentItem { get; }

        public int MaxDepth
        {
            get
            {
                int max = 0;
                foreach (var item in this)
                    max = Math.Max(max, item.Depth);
                return max;
            }
        }

        public int BottomItemsCount
        {
            get
            {
                int sum = 0;
                foreach (var item in this)
                {
                    if (!item.HasChildren)
                        sum++;
                    else
                        sum += item.Children.BottomItemsCount;
                }
                return sum;
            }
        }

        public double TotalWidth
        {
            get
            {
                double total = 0;
                foreach (var item in this)
                {
                    if (item.Children.Count > 0)
                        total += item.Children.TotalWidth;
                    else
                    {
                        try
                        {
                            var width = item.Width;
                            if (!double.IsNaN(width) && item.Visibility != System.Windows.Visibility.Collapsed)
                                total += width;
                        }
                        catch (ArgumentException ex)
                        {
                            // Пропускаем виртуализированные элементы
                            System.Diagnostics.Debug.WriteLine($"Skipping virtualized item: {ex.Message}");
                        }
                    }
                }
                return total;
            }
        }

        #endregion

        #region Public Methods

        public ColumnHeaderItem[] GetBottomItems()
        {
            var result = new List<ColumnHeaderItem>();
            foreach (var item in this)
                result.AddRange(GetBottomItems(item));
            return result.ToArray();
        }

        /// <summary>
        /// Перемещает элемент внутри этой же коллекции с одного индекса на другой.
        /// </summary>
        public void MoveItem(int fromIndex, int toIndex)
        {
            if (fromIndex < 0 || fromIndex >= Count)
                throw new ArgumentOutOfRangeException(nameof(fromIndex));
            if (toIndex < 0 || toIndex >= Count)
                throw new ArgumentOutOfRangeException(nameof(toIndex));
            if (fromIndex == toIndex)
                return;

            Move(fromIndex, toIndex);
        }

        /// <summary>
        /// Перемещает элемент из одной коллекции заголовков в другую.
        /// </summary>
        /// <param name="sourceCollection">Исходная коллекция.</param>
        /// <param name="targetCollection">Целевая коллекция.</param>
        /// <param name="item">Элемент для перемещения.</param>
        /// <param name="insertIndex">Индекс вставки в целевой коллекции. Если больше Count, вставка в конец.</param>
        public static void MoveToCollection(
            ColumnHeaderCollection sourceCollection,
            ColumnHeaderCollection targetCollection,
            ColumnHeaderItem item,
            int insertIndex)
        {
            if (sourceCollection == null)
                throw new ArgumentNullException(nameof(sourceCollection));
            if (targetCollection == null)
                throw new ArgumentNullException(nameof(targetCollection));
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            int sourceIndex = sourceCollection.IndexOf(item);
            if (sourceIndex < 0)
                return; // Элемент не найден в исходной коллекции

            sourceCollection.RemoveAt(sourceIndex);

            // Нормализуем индекс вставки
            if (insertIndex < 0)
                insertIndex = 0;
            if (insertIndex > targetCollection.Count)
                insertIndex = targetCollection.Count;

            targetCollection.Insert(insertIndex, item);
        }

        #endregion

        #region Private Methods

        private static ColumnHeaderItem[] GetBottomItems(ColumnHeaderItem item)
        {
            if (!item.HasChildren)
                return new[] { item };

            var result = new List<ColumnHeaderItem>();
            foreach (var child in item.Children)
                result.AddRange(GetBottomItems(child));
            return result.ToArray();
        }

        #endregion

        #region Protected Override Methods

        protected override void InsertItem(int index, ColumnHeaderItem item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            item.ParentItem = ParentItem;
            item.OwnerGrid = OwnerGrid;

            base.InsertItem(index, item);
        }

        protected override void ClearItems()
        {
            foreach (var item in this)
            {
                if (item != null)
                {
                    item.ParentItem = null;
                    item.OwnerGrid = null;
                }
            }

            base.ClearItems();
        }

        protected override void SetItem(int index, ColumnHeaderItem item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            var oldItem = this[index];
            oldItem.ParentItem = null;
            oldItem.OwnerGrid = null;

            item.ParentItem = ParentItem;
            item.OwnerGrid = OwnerGrid;

            base.SetItem(index, item);
        }

        #endregion
    }
}

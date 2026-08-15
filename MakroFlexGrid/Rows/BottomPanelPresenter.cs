using MakroFlexGrid.Core;
using MakroFlexGrid.Headers;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MakroFlexGrid.Rows
{
    /// <summary>
    /// Presenter для нижней панели с итоговыми значениями (BottomPanel).
    /// Управляет отображением, синхронизирует горизонтальный скролл со строками,
    /// вычисляет агрегаты для колонок с AggregateType != None.
    /// </summary>
    public class BottomPanelPresenter : FrameworkElement
    {
        #region Dependency Properties

        public static readonly DependencyProperty LeftFrozenColumnsCountProperty =
            DependencyProperty.Register(nameof(LeftFrozenColumnsCount), typeof(int),
            typeof(BottomPanelPresenter), new PropertyMetadata(0, OnSeparatorOrFrozenPropertyChanged));

        public static readonly DependencyProperty RightFrozenColumnsCountProperty =
            DependencyProperty.Register(nameof(RightFrozenColumnsCount), typeof(int),
            typeof(BottomPanelPresenter), new PropertyMetadata(0, OnSeparatorOrFrozenPropertyChanged));

        public static readonly DependencyProperty SeparatorWidthProperty =
            DependencyProperty.Register(nameof(SeparatorWidth), typeof(double),
            typeof(BottomPanelPresenter), new PropertyMetadata(0.0, OnSeparatorOrFrozenPropertyChanged));

        public static readonly DependencyProperty SeparatorBrushProperty =
            DependencyProperty.Register(nameof(SeparatorBrush), typeof(Brush),
            typeof(BottomPanelPresenter), new PropertyMetadata(Brushes.Gray, OnSeparatorOrFrozenPropertyChanged));

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable),
            typeof(BottomPanelPresenter), new PropertyMetadata(null, OnItemsSourceChanged));

        public static readonly DependencyProperty HasAggregatesProperty =
            DependencyProperty.Register(nameof(HasAggregates), typeof(bool),
            typeof(BottomPanelPresenter), new PropertyMetadata(false));

        public static readonly DependencyProperty ShowBottomCellBordersProperty =
            DependencyProperty.Register(nameof(ShowBottomCellBorders), typeof(bool),
            typeof(BottomPanelPresenter), new PropertyMetadata(true, OnShowBottomCellBordersChanged));

        public static readonly DependencyProperty BottomPanelHeightProperty =
            DependencyProperty.Register(nameof(BottomPanelHeight), typeof(double),
            typeof(BottomPanelPresenter), new PropertyMetadata(20.0));

        #endregion

        #region Properties

        public int LeftFrozenColumnsCount
        {
            get => (int)GetValue(LeftFrozenColumnsCountProperty);
            set => SetValue(LeftFrozenColumnsCountProperty, value);
        }

        public int RightFrozenColumnsCount
        {
            get => (int)GetValue(RightFrozenColumnsCountProperty);
            set => SetValue(RightFrozenColumnsCountProperty, value);
        }

        public double SeparatorWidth
        {
            get => (double)GetValue(SeparatorWidthProperty);
            set => SetValue(SeparatorWidthProperty, value);
        }

        public Brush SeparatorBrush
        {
            get => (Brush)GetValue(SeparatorBrushProperty);
            set => SetValue(SeparatorBrushProperty, value);
        }

        public IEnumerable ItemsSource
        {
            get => (IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public bool HasAggregates
        {
            get => (bool)GetValue(HasAggregatesProperty);
            set => SetValue(HasAggregatesProperty, value);
        }

        /// <summary>
        /// Показывать ли бордюры ячеек в нижней панели итогов.
        /// </summary>
        public bool ShowBottomCellBorders
        {
            get => (bool)GetValue(ShowBottomCellBordersProperty);
            set => SetValue(ShowBottomCellBordersProperty, value);
        }

        /// <summary>
        /// Высота нижней панели итогов. По умолчанию 20.
        /// </summary>
        public double BottomPanelHeight
        {
            get => (double)GetValue(BottomPanelHeightProperty);
            set => SetValue(BottomPanelHeightProperty, value);
        }

        public CustomDataGrid ParentGrid
        {
            get => _parentGrid;
            set => _parentGrid = value;
        }

        #endregion

        private CustomDataGrid _parentGrid;
        private ItemsControl _itemsControl;
        private BottomPanelViewModel _viewModel;

        public BottomPanelViewModel ViewModel
        {
            get => _viewModel;
            set
            {
                if (_viewModel != value)
                {
                    if (_viewModel != null)
                    {
                        _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
                    }
                    _viewModel = value;
                    if (_viewModel != null)
                    {
                        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
                    }
                    // We need to update the ItemsControl source because the ViewModel has changed
                    if (_itemsControl != null)
                    {
                        _itemsControl.ItemsSource = _viewModel == null ? null : new List<BottomPanelViewModel> { _viewModel };
                    }

                    // Sync properties from grid if they are available
                    SyncPropertiesFromGrid();
                }
            }
        }
        private bool _isLoadedSubscribed;
        private List<ColumnHeaderItem> _subscribedItems = new List<ColumnHeaderItem>();

        // Кэш PropertyInfo для быстрого доступа к значениям свойств
        private static readonly Dictionary<string, PropertyInfo> _propertyCache = new Dictionary<string, PropertyInfo>();

        /// <summary>
        /// Пустой DataTemplate для системной ячейки нижней панели.
        /// Используется чтобы ячейка занимала место, но ничего не отображала.
        /// </summary>
        private static readonly DataTemplate _emptyBottomCellTemplate = new DataTemplate();

        public BottomPanelPresenter()
        {
            _itemsControl = new ItemsControl
            {
                ItemTemplate = LoadBottomRowTemplate()
            };

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        #region Visual Tree

        protected override int VisualChildrenCount => _itemsControl != null ? 1 : 0;

        protected override Visual GetVisualChild(int index)
        {
            if (index == 0 && _itemsControl != null)
                return _itemsControl;
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        protected override IEnumerator LogicalChildren
        {
            get
            {
                if (_itemsControl != null)
                    yield return _itemsControl;
            }
        }

        #endregion

        #region Measure / Arrange

        protected override Size MeasureOverride(Size availableSize)
        {
            if (_itemsControl != null)
            {
                _itemsControl.Measure(availableSize);
                double height = Math.Max(_itemsControl.DesiredSize.Height, BottomPanelHeight);
                return new Size(availableSize.Width, height);
            }
            return base.MeasureOverride(availableSize);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (_itemsControl != null)
            {
                _itemsControl.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));
            }
            return finalSize;
        }

        #endregion

        #region Load / Unload

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (_isLoadedSubscribed) return;
            _isLoadedSubscribed = true;

            AddVisualChild(_itemsControl);

            if (_viewModel == null)
            {
                _viewModel = new BottomPanelViewModel();
                _itemsControl.ItemsSource = new List<BottomPanelViewModel> { _viewModel };
            }
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;

            // Подписываемся на ScrollManager для синхронизации горизонтального скролла
            if (_parentGrid != null)
            {
                var presenter = FindRowsPresenter();
                if (presenter?.ScrollManager != null)
                {
                    presenter.ScrollManager.HorizontalOffsetChanged += OnScrollManagerOffsetChanged;
                    _viewModel.HorizontalOffset = presenter.ScrollManager.HorizontalOffset;
                }

                // Подписываемся на изменения колонок
                _parentGrid.Columns.CollectionChanged += OnColumnsCollectionChanged;
                _parentGrid.SizeChanged += OnParentGridSizeChanged;

                // Подписываемся на изменения видимости колонок
                SubscribeToColumnVisibilityChanges();

                // Первоначальное вычисление агрегатов
                UpdateAggregates();
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _isLoadedSubscribed = false;

            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;

            if (_parentGrid != null)
            {
                var presenter = FindRowsPresenter();
                if (presenter?.ScrollManager != null)
                {
                    presenter.ScrollManager.HorizontalOffsetChanged -= OnScrollManagerOffsetChanged;
                }

                _parentGrid.Columns.CollectionChanged -= OnColumnsCollectionChanged;
                _parentGrid.SizeChanged -= OnParentGridSizeChanged;

                // Отписываемся от изменений видимости колонок
                UnsubscribeFromColumnVisibilityChanges();
            }

            RemoveVisualChild(_itemsControl);
        }

        #endregion

        #region Scroll Synchronization

        private void OnScrollManagerOffsetChanged(double offset)
        {
            _viewModel.HorizontalOffset = offset;
        }

        #endregion

        #region Column Visibility Subscription

        /// <summary>
        /// Подписывается на изменения видимости всех листовых колонок.
        /// </summary>
        private void SubscribeToColumnVisibilityChanges()
        {
            if (_parentGrid == null) return;

            var descriptor = DependencyPropertyDescriptor.FromProperty(
                ColumnHeaderItem.IsVisibleProperty, typeof(ColumnHeaderItem));

            if (descriptor == null) return;

            // Получаем все листовые колонки
            var allLeafItems = GetAllLeafColumnItems();

            foreach (var item in allLeafItems)
            {
                if (!_subscribedItems.Contains(item))
                {
                    descriptor.AddValueChanged(item, OnColumnVisibilityChanged);
                    _subscribedItems.Add(item);
                }
            }
        }

        /// <summary>
        /// Отписывается от изменений видимости колонок.
        /// </summary>
        private void UnsubscribeFromColumnVisibilityChanges()
        {
            var descriptor = DependencyPropertyDescriptor.FromProperty(
                ColumnHeaderItem.IsVisibleProperty, typeof(ColumnHeaderItem));

            if (descriptor == null) return;

            foreach (var item in _subscribedItems)
            {
                descriptor.RemoveValueChanged(item, OnColumnVisibilityChanged);
            }
            _subscribedItems.Clear();
        }

        /// <summary>
        /// Обработчик изменения видимости колонки.
        /// </summary>
        private void OnColumnVisibilityChanged(object sender, EventArgs e)
        {
            // При изменении видимости пересчитываем агрегаты
            UpdateAggregates();
        }

        /// <summary>
        /// Возвращает все листовые колонки из всех коллекций.
        /// </summary>
        private List<ColumnHeaderItem> GetAllLeafColumnItems()
        {
            var result = new List<ColumnHeaderItem>();

            if (_parentGrid == null) return result;

            result.AddRange(_parentGrid.FrozenColumnHeaders.GetBottomItems());
            result.AddRange(_parentGrid.ScrollableColumnHeaders.GetBottomItems());
            result.AddRange(_parentGrid.RightFrozenColumnHeaders.GetBottomItems());

            return result;
        }

        #endregion

        #region Column / Size Changes

        private void OnColumnsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // При изменении коллекции колонок обновляем подписки
            SubscribeToColumnVisibilityChanges();
            UpdateAggregates();
        }

        private void OnParentGridSizeChanged(object sender, SizeChangedEventArgs e)
        {
            // При изменении размера грида обновляем ширину строки.
            // Ширина ячеек обновится автоматически через подписку BottomCellViewModel
            // на DataGridColumn.ActualWidthProperty.
            UpdateRowWidth();
        }

        #endregion

        #region Aggregates Calculation

        /// <summary>
        /// Вычисляет итоговые значения для всех колонок и обновляет ViewModel.
        /// </summary>
        public void UpdateAggregates()
        {
            if (_parentGrid == null || !IsLoaded || _viewModel == null) return;

            var columns = _parentGrid.Columns;
            if (columns == null || columns.Count == 0) return;

            // Получаем листовые элементы заголовков из коллекций (только видимые!)
            var frozenBottomItems = _parentGrid.FrozenColumnHeaders.GetBottomItems()
                .Where(item => item.IsVisible).ToList();

            var scrollableBottomItems = _parentGrid.ScrollableColumnHeaders.GetBottomItems()
                .Where(item => item.IsVisible).ToList();

            var rightFrozenBottomItems = _parentGrid.RightFrozenColumnHeaders.GetBottomItems()
                .Where(item => item.IsVisible).ToList();

            // Получаем элементы для агрегации
            var items = GetItemsList();

            // Очищаем старые ячейки (с отпиской от событий ширины колонок)
            foreach (var cell in _viewModel.LeftCells) cell.Cleanup();
            foreach (var cell in _viewModel.CenterCells) cell.Cleanup();
            foreach (var cell in _viewModel.RightCells) cell.Cleanup();
            _viewModel.LeftCells.Clear();
            _viewModel.CenterCells.Clear();
            _viewModel.RightCells.Clear();

            // Создаём ячейки для левых frozen колонок
            // Добавляем системную ячейку (ширина 30px) в начало левой панели (если включена).
            // Ячейка использует пустой Template, чтобы ничего не отображать — только занимать место.
            if (_parentGrid != null && _parentGrid.IsSystemColumnEnabled)
            {
                _viewModel.LeftCells.Add(new BottomCellViewModel
                {
                    Width = 15,
                    GridLineBrush = _parentGrid?.GridLineBrush ?? Brushes.LightGray,
                    Value = "",
                    Template = _emptyBottomCellTemplate
                });
            }

            foreach (var headerItem in frozenBottomItems)
            {
                if (headerItem.SyncColumn != null)
                {
                    var cell = CreateCellForColumn(headerItem.SyncColumn, items, headerItem);
                    _viewModel.LeftCells.Add(cell);
                }
            }

            // Создаём ячейки для центральных scrollable колонок
            foreach (var headerItem in scrollableBottomItems)
            {
                if (headerItem.SyncColumn != null)
                {
                    var cell = CreateCellForColumn(headerItem.SyncColumn, items, headerItem);
                    _viewModel.CenterCells.Add(cell);
                }
            }

            // Создаём ячейки для правых frozen колонок
            foreach (var headerItem in rightFrozenBottomItems)
            {
                if (headerItem.SyncColumn != null)
                {
                    var cell = CreateCellForColumn(headerItem.SyncColumn, items, headerItem);
                    _viewModel.RightCells.Add(cell);
                }
            }

            // Первая (самая левая) ячейка правой frozen-панели должна иметь левый бордюр
            if (_viewModel.RightCells.Count > 0)
                _viewModel.RightCells[0].IsLeftmostInRightPanel = true;

            // Проверяем, есть ли хотя бы одна ячейка с непустым итоговым значением
            bool hasAggregates = false;
            foreach (var cell in _viewModel.LeftCells)
            {
                if (!string.IsNullOrEmpty(cell.Value))
                {
                    hasAggregates = true;
                    break;
                }
            }
            if (!hasAggregates)
            {
                foreach (var cell in _viewModel.CenterCells)
                {
                    if (!string.IsNullOrEmpty(cell.Value))
                    {
                        hasAggregates = true;
                        break;
                    }
                }
            }
            if (!hasAggregates)
            {
                foreach (var cell in _viewModel.RightCells)
                {
                    if (!string.IsNullOrEmpty(cell.Value))
                    {
                        hasAggregates = true;
                        break;
                    }
                }
            }
            bool hasPanelText = !string.IsNullOrWhiteSpace(_parentGrid.BottomPanelText);
            _viewModel.HasAggregates = hasAggregates || hasPanelText;

            // Синхронизируем свойства из грида
            SyncPropertiesFromGrid();

            // Обновляем ширину строки
            UpdateRowWidth();
        }

        /// <summary>
        /// Создаёт ячейку для указанной колонки, вычисляя агрегат если нужно.
        /// </summary>
        private BottomCellViewModel CreateCellForColumn(DataGridColumn column, List<object> items, ColumnHeaderItem headerItem)
        {
            var cell = new BottomCellViewModel
            {
                Width = column.ActualWidth,
                GridLineBrush = _parentGrid?.GridLineBrush ?? Brushes.LightGray,
                Value = ""
            };

            // Подписываемся на изменения ширины колонки
            cell.SetColumn(column);

            if (headerItem != null)
            {
                // Передаём кастомный DataTemplate для визуального отображения ячейки нижней панели.
                // Если BottomCellTemplate не задан, используем CellTemplate (для обратной совместимости).
                // Если задан BottomCellTemplate — используем только его.
                cell.Template = headerItem.BottomCellTemplate ?? headerItem.CellTemplate;

                if (headerItem.AggregateType != Headers.AggregateType.None && items.Count > 0)
                {
                    cell.Value = CalculateAggregate(headerItem, items);
                }
            }

            return cell;
        }

        /// <summary>
        /// Вычисляет агрегат для указанного заголовка колонки по всем элементам.
        /// </summary>
        private string CalculateAggregate(ColumnHeaderItem headerItem, List<object> items)
        {
            var sortMemberPath = headerItem.SortMemberPath;
            if (string.IsNullOrEmpty(sortMemberPath)) return "";

            var aggregateType = headerItem.AggregateType;

            // Если колонка скрыта, не считаем для неё агрегат
            if (!headerItem.IsVisible) return "";

            switch (aggregateType)
            {
                case Headers.AggregateType.Count:
                    // Count считаем количество НЕПУСТЫХ значений в колонке
                    return CalculateCount(items, sortMemberPath).ToString();

                case Headers.AggregateType.Sum:
                case Headers.AggregateType.Average:
                case Headers.AggregateType.Min:
                case Headers.AggregateType.Max:
                    return CalculateNumericAggregate(aggregateType, items, sortMemberPath);

                default:
                    return "";
            }
        }

        /// <summary>
        /// Вычисляет количество непустых значений в колонке.
        /// </summary>
        private int CalculateCount(List<object> items, string sortMemberPath)
        {
            int count = 0;
            foreach (var item in items)
            {
                if (item == null) continue;
                var value = GetPropertyValue(item, sortMemberPath);
                if (value != null && !string.IsNullOrEmpty(value.ToString()))
                {
                    count++;
                }
            }
            return count;
        }

        /// <summary>
        /// Вычисляет числовой агрегат (Sum, Average, Min, Max).
        /// </summary>
        private string CalculateNumericAggregate(Headers.AggregateType aggregateType, List<object> items, string sortMemberPath)
        {
            double? sum = null;
            double? min = null;
            double? max = null;
            int count = 0;

            foreach (var item in items)
            {
                if (item == null) continue;

                var value = GetPropertyValue(item, sortMemberPath);
                if (value == null) continue;

                if (double.TryParse(value.ToString(), out double numericValue))
                {
                    count++;
                    sum = (sum ?? 0) + numericValue;
                    min = min.HasValue ? Math.Min(min.Value, numericValue) : numericValue;
                    max = max.HasValue ? Math.Max(max.Value, numericValue) : numericValue;
                }
            }

            switch (aggregateType)
            {
                case Headers.AggregateType.Sum:
                    return sum?.ToString("F2") ?? "";

                case Headers.AggregateType.Average:
                    if (count > 0 && sum.HasValue)
                        return (sum.Value / count).ToString("F2");
                    return "";

                case Headers.AggregateType.Min:
                    return min?.ToString("F2") ?? "";

                case Headers.AggregateType.Max:
                    return max?.ToString("F2") ?? "";

                default:
                    return "";
            }
        }

        /// <summary>
        /// Получает значение свойства объекта по имени через рефлексию с кэшированием.
        /// </summary>
        private static object GetPropertyValue(object item, string propertyName)
        {
            if (item == null || string.IsNullOrEmpty(propertyName)) return null;

            var type = item.GetType();
            var cacheKey = $"{type.FullName}.{propertyName}";

            if (!_propertyCache.TryGetValue(cacheKey, out var property))
            {
                property = type.GetProperty(propertyName);
                if (property != null)
                {
                    _propertyCache[cacheKey] = property;
                }
            }

            return property?.GetValue(item);
        }

        /// <summary>
        /// Получает список элементов из ItemsSource.
        /// </summary>
        private List<object> GetItemsList()
        {
            var result = new List<object>();
            if (ItemsSource == null) return result;

            foreach (var item in ItemsSource)
            {
                if (item != null)
                    result.Add(item);
            }

            return result;
        }

        #endregion

        #region Sync Properties

        /// <summary>
        /// Синхронизирует свойства из родительского грида в ViewModel.
        /// </summary>
        private void SyncPropertiesFromGrid()
        {
            if (_parentGrid == null) return;

            if (_viewModel == null) return;

            _viewModel.LeftPanelBackground = _parentGrid.BottomPanelBackground;
            _viewModel.CenterPanelBackground = _parentGrid.BottomPanelBackground;
            _viewModel.RightPanelBackground = _parentGrid.BottomPanelBackground;
            _viewModel.BottomPanelBackground = _parentGrid.BottomPanelBackground;
            _viewModel.GridLineBrush = _parentGrid.GridLineBrush;
            _viewModel.SeparatorWidth = SeparatorWidth;
            _viewModel.SeparatorBrush = SeparatorBrush;
            _viewModel.ShowBottomCellBorders = ShowBottomCellBorders;
            _viewModel.PanelText = _parentGrid.BottomPanelText;
            _viewModel.LeftFrozenColumnsCount = LeftFrozenColumnsCount;
            _viewModel.RightFrozenColumnsCount = RightFrozenColumnsCount;

            _viewModel.PanelTextAlignment = _parentGrid.PanelTextAlignment;
            _viewModel.TextPosition = _parentGrid.BottomPanelTextPosition;
            _viewModel.PanelTextPadding = _parentGrid.PanelTextPadding;
            _viewModel.PanelTextTemplate = _parentGrid.PanelTextTemplate;
        }

        /// <summary>
        /// Обновляет ширину строки панели.
        /// </summary>
        public void UpdateRowWidth()
        {
            if (_parentGrid == null) return;

            double gridWidth = _parentGrid.ActualWidth;

            // Находим UnifiedRowsPresenter для получения ширины скроллбара
            var presenter = FindRowsPresenter();
            double scrollBarWidth = presenter?.VerticalScrollBarWidth ?? 0;

            if (_viewModel != null)
            {
                _viewModel.RowWidth = Math.Max(0, gridWidth - scrollBarWidth - 2);
            }
        }

        /// <summary>
        /// Находит UnifiedRowsPresenter в визуальном дереве.
        /// </summary>
        private UnifiedRowsPresenter FindRowsPresenter()
        {
            if (_parentGrid == null) return null;

            // Ищем через TemplateChild
            var field = typeof(CustomDataGrid).GetField("_unifiedRowsPresenter",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return field?.GetValue(_parentGrid) as UnifiedRowsPresenter;
        }

        #endregion

        #region Template Loading

        private DataTemplate LoadBottomRowTemplate()
        {
            try
            {
                var uri = new Uri("/MakroFlexGrid;component/Themes/RowTemplates.xaml", UriKind.RelativeOrAbsolute);
                var resourceDictionary = new ResourceDictionary
                {
                    Source = uri
                };

                return resourceDictionary["BottomRowTemplate"] as DataTemplate;
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region Event Handlers

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var presenter = (BottomPanelPresenter)d;
            presenter.UpdateAggregates();
        }

        /// <summary>
        /// Обработчик изменения свойств, влияющих на отображение разделителей
        /// и замороженных панелей. Синхронизирует актуальные значения в ViewModel,
        /// чтобы разделители замороженных зон отображались корректно.
        /// </summary>
        private static void OnSeparatorOrFrozenPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var presenter = (BottomPanelPresenter)d;
            presenter.SyncPropertiesFromGrid();
        }

        private static void OnShowBottomCellBordersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var presenter = (BottomPanelPresenter)d;
            if (presenter._viewModel != null)
            {
                presenter._viewModel.ShowBottomCellBorders = (bool)e.NewValue;
            }
        }

        /// <summary>
        /// Синхронизирует HasAggregates из ViewModel в DependencyProperty презентера
        /// для привязки Visibility в шаблоне.
        /// </summary>
        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(BottomPanelViewModel.HasAggregates))
            {
                HasAggregates = _viewModel.HasAggregates;
            }
        }

        #endregion
    }
}

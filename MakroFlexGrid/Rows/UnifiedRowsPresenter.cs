using MakroFlexGrid.Core;
using MakroFlexGrid.Scroll;
using MakroFlexGrid.Utilities;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MakroFlexGrid.Rows
{
    public class UnifiedRowsPresenter : FrameworkElement
    {
        public static readonly DependencyProperty LeftFrozenColumnsCountProperty =
            DependencyProperty.Register(nameof(LeftFrozenColumnsCount), typeof(int),
            typeof(UnifiedRowsPresenter),
            new PropertyMetadata(0, OnPropertyChangedThrottled));

        public static readonly DependencyProperty RightFrozenColumnsCountProperty =
            DependencyProperty.Register(nameof(RightFrozenColumnsCount), typeof(int),
            typeof(UnifiedRowsPresenter),
            new PropertyMetadata(0, OnPropertyChangedThrottled));

        public static readonly DependencyProperty HorizontalOffsetProperty =
            DependencyProperty.Register(nameof(HorizontalOffset), typeof(double),
            typeof(UnifiedRowsPresenter),
            new PropertyMetadata(0.0));

        public static readonly DependencyProperty MaxHorizontalOffsetProperty =
            DependencyProperty.Register(nameof(MaxHorizontalOffset), typeof(double),
            typeof(UnifiedRowsPresenter),
            new PropertyMetadata(0.0));

        public static readonly DependencyProperty VerticalOffsetProperty =
            DependencyProperty.Register(nameof(VerticalOffset), typeof(double),
            typeof(UnifiedRowsPresenter),
            new PropertyMetadata(0.0, OnVerticalOffsetChanged));

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable),
            typeof(UnifiedRowsPresenter),
            new PropertyMetadata(null, OnItemsSourceChanged));

        public static readonly DependencyProperty VerticalScrollBarWidthProperty =
            DependencyProperty.Register(nameof(VerticalScrollBarWidth), typeof(double),
            typeof(UnifiedRowsPresenter),
            new PropertyMetadata(0.0));

        public static readonly DependencyProperty SeparatorWidthProperty =
            DependencyProperty.Register(nameof(SeparatorWidth), typeof(double),
            typeof(UnifiedRowsPresenter),
            new PropertyMetadata(0.0));

        public static readonly DependencyProperty SeparatorBrushProperty =
            DependencyProperty.Register(nameof(SeparatorBrush), typeof(Brush),
            typeof(UnifiedRowsPresenter),
            new PropertyMetadata(Brushes.Gray));

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

        public IEnumerable ItemsSource
        {
            get => (IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public double HorizontalOffset
        {
            get => (double)GetValue(HorizontalOffsetProperty);
            set => SetValue(HorizontalOffsetProperty, value);
        }

        public double MaxHorizontalOffset
        {
            get => (double)GetValue(MaxHorizontalOffsetProperty);
            set => SetValue(MaxHorizontalOffsetProperty, value);
        }

        public double VerticalOffset
        {
            get => (double)GetValue(VerticalOffsetProperty);
            set => SetValue(VerticalOffsetProperty, value);
        }

        public double VerticalScrollBarWidth
        {
            get => (double)GetValue(VerticalScrollBarWidthProperty);
            set => SetValue(VerticalScrollBarWidthProperty, value);
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

        public CustomDataGrid ParentGrid
        {
            get => _parentGrid;
            set => _parentGrid = value;
        }
        private CustomDataGrid _parentGrid;
        internal UnifiedRowsItemsControl ItemsControl => _itemsControl;
        private UnifiedRowsItemsControl _itemsControl;

        // WeakDependencyPropertyListener для предотвращения утечек при сборке GC
        private WeakDependencyPropertyListener _gridLineBrushListener;
        private WeakDependencyPropertyListener _separatorWidthListener;
        private WeakDependencyPropertyListener _separatorBrushListener;

        public void UpdateRowSelection(object selectedItem)
        {
            if (_itemsControl == null) return;

            for (int i = 0; i < _itemsControl.Items.Count; i++)
            {
                var container = _itemsControl.ItemContainerGenerator.ContainerFromIndex(i) as RowContainer;
                if (container?.DataContext is RowViewModel vm)
                {
                    vm.IsSelected = ReferenceEquals(vm.Item, selectedItem);
                }
            }
        }

        /// <summary>
        /// Обновляет состояние IsSelected для всех строк в режиме Multiple.
        /// Устанавливает IsSelected = true для элементов, присутствующих в selectedItems.
        /// </summary>
        public void UpdateMultipleSelection(HashSet<object> selectedItems)
        {
            if (_itemsControl == null) return;

            for (int i = 0; i < _itemsControl.Items.Count; i++)
            {
                var container = _itemsControl.ItemContainerGenerator.ContainerFromIndex(i) as RowContainer;
                if (container?.DataContext is RowViewModel vm)
                {
                    vm.IsSelected = selectedItems.Contains(vm.Item);
                }
            }
        }

        /// <summary>
        /// Обновляет BottomRowTemplate во всех активных RowViewModel при изменении в CustomDataGrid.
        /// </summary>
        internal void UpdateRowBottomTemplate()
        {
            if (_itemsControl == null || _parentGrid == null) return;

            var template = _parentGrid.BottomRowTemplate;
            for (int i = 0; i < _itemsControl.Items.Count; i++)
            {
                var container = _itemsControl.ItemContainerGenerator.ContainerFromIndex(i) as RowContainer;
                if (container?.DataContext is RowViewModel vm)
                {
                    vm.BottomRowTemplate = template;
                }
            }
        }
        private bool _isUpdatingScroll = false;
        private bool _isMeasuring = false;
        private bool _isUpdatingMaxOffset = false;
        private bool _isLoadedSubscribed = false;
        private double _lastMeasureHeight = double.PositiveInfinity;

        private ScrollManager _scrollManager;
        private ScrollViewerManager _scrollViewerManager;

        private DateTime _lastStatsLog = DateTime.MinValue;

        public ScrollManager ScrollManager => _scrollManager;

        public event EventHandler<double> HorizontalScrollChanged;
        public event EventHandler<double> VerticalScrollChanged;


        public UnifiedRowsPresenter()
        {
            _scrollManager = new ScrollManager();

            _itemsControl = new UnifiedRowsItemsControl(this);
            _itemsControl.VerticalScrollChanged += OnVerticalScrollChanged;

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;

            // ScrollManager - централизованный сервис для горизонтального offset.
            // Подписываемся на событие HorizontalOffsetChanged.
            _scrollManager.HorizontalOffsetChanged += OnScrollManagerOffsetChanged;
        }

        /// <summary>
        /// Обработчик изменения горизонтального смещения.
        /// Вызывается из ScrollManager.HorizontalOffsetChanged.
        /// </summary>
        private void OnScrollManagerOffsetChanged(double offset)
        {

            HorizontalOffset = offset;


            // Не синхронизируем ItemsControl
            // _itemsControl?.ScrollToHorizontalOffset(offset);

            HorizontalScrollChanged?.Invoke(this, offset);
        }

        private void OnVerticalScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (_isUpdatingScroll) return;

            _isUpdatingScroll = true;
            try
            {
                var newOffset = _itemsControl.VerticalOffset;

                VerticalOffset = newOffset;
                VerticalScrollChanged?.Invoke(this, VerticalOffset);

            }
            finally
            {
                _isUpdatingScroll = false;
            }

            // UpdateVerticalScrollBarWidth обновляет только свойство и UI,
            // полагаясь на вычисленное значение (VerticalScrollBarWidth),
            // а не вызывает повторную прокрутку, если ширина не изменилась.
            UpdateVerticalScrollBarWidth();

            // LogVirtualizationStats работает с throttling'ом (раз в секунду),
            // чтобы не нагружать производительность на Items при каждой прокрутке.
            LogVirtualizationStats();
        }


        public void SetParentGrid(CustomDataGrid grid)
        {
            _parentGrid = grid;
        }

        public void UpdateMaxHorizontalOffset()
        {
            if (_parentGrid == null) return;

            double centerColumnsWidth = 0;
            int leftCount = Math.Min(LeftFrozenColumnsCount, _parentGrid.Columns.Count);
            int rightCount = Math.Min(RightFrozenColumnsCount, _parentGrid.Columns.Count);
            int centerStart = leftCount;
            int centerEnd = _parentGrid.Columns.Count - rightCount;

            for (int i = centerStart; i < centerEnd; i++)
            {
                centerColumnsWidth += _parentGrid.Columns[i].ActualWidth;
            }

            double viewportWidth = _parentGrid.ActualWidth;

            double leftWidth = 0;
            for (int i = 0; i < leftCount; i++)
                leftWidth += _parentGrid.Columns[i].ActualWidth;

            double rightWidth = 0;
            for (int i = centerEnd; i < _parentGrid.Columns.Count; i++)
                rightWidth += _parentGrid.Columns[i].ActualWidth;

            // Учитываем ширину разделителей (SeparatorWidth * 2), так как CenterColDef
            // в RowTemplates.xaml имеет Width="*" и вычисляется как:
            // MainGrid.ActualWidth - LeftColDef - Separator - RightColDef - Separator
            double separatorWidth = SeparatorWidth;
            double availableWidth = viewportWidth - leftWidth - rightWidth - separatorWidth * 2;
            double maxOffset = Math.Max(0, centerColumnsWidth - availableWidth);

            _scrollManager.MaxHorizontalOffset = maxOffset;
            MaxHorizontalOffset = maxOffset;

            // Если текущий HorizontalOffset превышает новый максимум - корректируем
            if (_scrollManager.HorizontalOffset > maxOffset)
            {
                _scrollManager.HorizontalOffset = maxOffset;
            }
        }

        /// <summary>
        /// Принудительная синхронизация горизонтального смещения.
        /// Используется после операций, когда Items.Refresh() может сбросить
        /// HorizontalOffset в ScrollViewer.
        /// </summary>
        public void ForceSyncScroll(double offset)
        {
            if (_itemsControl == null) return;

            // Принудительно устанавливаем offset в ScrollViewer.
            // ScrollViewer.ScrollToHorizontalOffset > ScrollChanged > ScrollViewerManager
            // > ScrollManager.HorizontalOffset > HorizontalOffsetChanged > OnScrollManagerOffsetChanged
            // > HorizontalOffset DP (синхронизация визуального слоя).
            // Важно: установка HorizontalOffset DP не вызывает повторную синхронизацию с прокруткой.
            _itemsControl.ScrollToHorizontalOffset(offset);
        }

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

        protected override Size MeasureOverride(Size availableSize)
        {
            // Guard: предотвращение рекурсивного вызова (measure loop)
            if (_isMeasuring)
            {
                return availableSize;
            }

            if (_itemsControl != null)
            {
                _isMeasuring = true;
                try
                {
                    // Ограничиваем высоту, чтобы VirtualizingStackPanel получил
                    // корректный viewport. Если availableSize.Height бесконечность,
                    // используем текущую высоту.
                    var constrainedSize = availableSize;
                    if (double.IsInfinity(constrainedSize.Height))
                    {
                        constrainedSize.Height = Math.Max(ActualHeight, 100);
                        System.Diagnostics.Debug.WriteLine($"[UnifiedRowsPresenter] MeasureOverride: constrained height from INF to {constrainedSize.Height}");
                    }

                    // Если высота уменьшилась относительно предыдущего замера ItemsControl,
                    // чтобы VirtualizingStackPanel пересчитал свои строки заново.
                    // Иначе VirtualizingStackPanel не переиспользует контейнеры строк,
                    // если они еще находятся в старом viewport, а просто сдвигает их вверх.
                    if (constrainedSize.Height < _lastMeasureHeight - 0.5)
                    {
                        _itemsControl.InvalidateMeasure();
                    }
                    _lastMeasureHeight = constrainedSize.Height;

                    _itemsControl.Measure(constrainedSize);

                    // Важно: возвращаем constrainedSize (availableSize), а не _itemsControl.DesiredSize.
                    // UnifiedRowsPresenter должен занимать всю доступную высоту родительского Grid.
                    // Иначе DesiredSize может вызвать рекурсивный пересчет (measure loop),
                    // т.к. WPF будет думать, что нужно больше места.
                    return constrainedSize;
                }
                finally
                {
                    _isMeasuring = false;
                }
            }
            return base.MeasureOverride(availableSize);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (_itemsControl != null)
            {
                _itemsControl.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));
            }

            // После того как layout завершен и ActualWidth/ActualHeight обновлены,
            // обновляем MaxHorizontalOffset.
            // Dispatcher.BeginInvoke не используется, т.к. синхронный вызов безопасен
            // и не вызывает рекурсивный пересчет (measure loop).
            if (finalSize.Width > 0 && finalSize.Height > 0 && !_isUpdatingMaxOffset)
            {
                _isUpdatingMaxOffset = true;
                try
                {
                    UpdateMaxHorizontalOffset();
                }
                finally
                {
                    _isUpdatingMaxOffset = false;
                }
            }

            return finalSize;
        }

        private void UpdateVerticalScrollBarWidth()
        {
            if (_itemsControl == null) return;

            var visibility = _itemsControl.ComputedVerticalScrollBarVisibility;
            double width = visibility == Visibility.Visible ? SystemParameters.VerticalScrollBarWidth : 0.0;

            VerticalScrollBarWidth = width;
            _parentGrid?.UpdateScrollBarVisibility();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Защита от повторного вызова OnLoaded (например, при show/hide).
            // Флаг гарантирует однократную подписку на события.
            if (_isLoadedSubscribed) return;
            _isLoadedSubscribed = true;

            AddVisualChild(_itemsControl);

            // Создаем ScrollViewerManager для подписки на скролл и синхронизации.
            // Это необходимо, так как при show/hide может измениться
            // внутренний ScrollViewer, и без этого синхронизация с ScrollManager сбросится.
            if (_scrollViewerManager == null)
            {
                _scrollViewerManager = new ScrollViewerManager(_scrollManager);
            }

            if (_parentGrid != null)
            {
                UpdateRows();
                UpdateMaxHorizontalOffset();

                // Переподписываемся на события колонок и размера (защита от сброса
                // при show/hide, если _isLoadedSubscribed не сработал).
                _parentGrid.Columns.CollectionChanged -= OnColumnsCollectionChanged;
                _parentGrid.Columns.CollectionChanged += OnColumnsCollectionChanged;
                _parentGrid.SizeChanged -= OnParentGridSizeChanged;
                _parentGrid.SizeChanged += OnParentGridSizeChanged;

                // Удаляем старые WeakDependencyPropertyListener перед созданием новых,
                // чтобы избежать дублирования подписок при повторном OnLoaded.
                DisposeWeakListeners();

                // Подписываемся на изменение GridLineBrush в CustomDataGrid через WeakDependencyPropertyListener.
                // Предотвращает утечки памяти при сборке GC контекста, удерживая слабую ссылку
                // вместо сильной через EventHandlerStore.
                var gridLineDescriptor = DependencyPropertyDescriptor.FromProperty(
                    CustomDataGrid.GridLineBrushProperty, typeof(CustomDataGrid));
                if (gridLineDescriptor != null)
                {
                    _gridLineBrushListener = new WeakDependencyPropertyListener(
                        gridLineDescriptor, _parentGrid, this, OnGridLineBrushChanged);
#if DEBUG
                    MemoryDiagnostics.OnWeakSubscriptionCreated();
#endif
                }

                // Подписываемся на изменение SeparatorWidth через WeakDependencyPropertyListener
                var sepWidthDescriptor = DependencyPropertyDescriptor.FromProperty(
                    CustomDataGrid.SeparatorWidthProperty, typeof(CustomDataGrid));
                if (sepWidthDescriptor != null)
                {
                    _separatorWidthListener = new WeakDependencyPropertyListener(
                        sepWidthDescriptor, _parentGrid, this, OnSeparatorChanged);
#if DEBUG
                    MemoryDiagnostics.OnWeakSubscriptionCreated();
#endif
                }

                // Подписываемся на изменение SeparatorBrush через WeakDependencyPropertyListener
                var sepBrushDescriptor = DependencyPropertyDescriptor.FromProperty(
                    CustomDataGrid.SeparatorBrushProperty, typeof(CustomDataGrid));
                if (sepBrushDescriptor != null)
                {
                    _separatorBrushListener = new WeakDependencyPropertyListener(
                        sepBrushDescriptor, _parentGrid, this, OnSeparatorChanged);
#if DEBUG
                    MemoryDiagnostics.OnWeakSubscriptionCreated();
#endif
                }
            }
        }

        /// <summary>
        /// Распространяет GridLineBrush на все активные RowViewModel при изменении в CustomDataGrid.
        /// </summary>
        private void OnGridLineBrushChanged(object sender, EventArgs e)
        {
            if (_itemsControl == null || _parentGrid == null) return;

            var brush = _parentGrid.GridLineBrush;
            for (int i = 0; i < _itemsControl.Items.Count; i++)
            {
                var container = _itemsControl.ItemContainerGenerator.ContainerFromIndex(i) as RowContainer;
                if (container?.DataContext is RowViewModel vm)
                {
                    vm.GridLineBrush = brush;
                }
            }
        }

        /// <summary>
        /// Распространяет SeparatorWidth/SeparatorBrush на все активные RowViewModel
        /// при изменении в CustomDataGrid.
        /// </summary>
        private void OnSeparatorChanged(object sender, EventArgs e)
        {
            if (_itemsControl == null || _parentGrid == null) return;

            double width = _parentGrid.SeparatorWidth;
            Brush brush = _parentGrid.SeparatorBrush;
            for (int i = 0; i < _itemsControl.Items.Count; i++)
            {
                var container = _itemsControl.ItemContainerGenerator.ContainerFromIndex(i) as RowContainer;
                if (container?.DataContext is RowViewModel vm)
                {
                    vm.SeparatorWidth = width;
                    vm.SeparatorBrush = brush;
                }
            }
        }

        private void OnParentGridSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateMaxHorizontalOffset();
        }

        /// <summary>
        /// Освобождает и обнуляет все WeakDependencyPropertyListener.
        /// Вызывается при повторной загрузке в OnLoaded и при выгрузке в OnUnloaded.
        /// </summary>
        private void DisposeWeakListeners()
        {
            if (_gridLineBrushListener != null)
            {
                _gridLineBrushListener.Dispose();
                _gridLineBrushListener = null;
#if DEBUG
                MemoryDiagnostics.OnWeakSubscriptionDisposed();
#endif
            }

            if (_separatorWidthListener != null)
            {
                _separatorWidthListener.Dispose();
                _separatorWidthListener = null;
#if DEBUG
                MemoryDiagnostics.OnWeakSubscriptionDisposed();
#endif
            }

            if (_separatorBrushListener != null)
            {
                _separatorBrushListener.Dispose();
                _separatorBrushListener = null;
#if DEBUG
                MemoryDiagnostics.OnWeakSubscriptionDisposed();
#endif
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _isLoadedSubscribed = false;

            if (_itemsControl != null)
            {
                _itemsControl.VerticalScrollChanged -= OnVerticalScrollChanged;
            }

            if (_scrollManager != null)
            {
                _scrollManager.HorizontalOffsetChanged -= OnScrollManagerOffsetChanged;
            }

            // Освобождаем все WeakDependencyPropertyListener перед выгрузкой
            DisposeWeakListeners();

            // Если не вызвать Dispose() у ScrollViewerManager, то отписка
            // от ScrollManager.HorizontalOffsetChanged не сработает, и синхронизация
            // ScrollViewer останется активной. Это важно при show/hide, чтобы при повторной
            // загрузке не было дублирования подписок.
            if (_scrollViewerManager != null)
            {
                _scrollViewerManager.Dispose();
                _scrollViewerManager = null;
            }

            if (_parentGrid != null)
            {
                _parentGrid.Columns.CollectionChanged -= OnColumnsCollectionChanged;
                _parentGrid.SizeChanged -= OnParentGridSizeChanged;
            }

            RemoveVisualChild(_itemsControl);

            _itemsControl.ItemsSource = null;
        }

        private void OnColumnsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateMaxHorizontalOffset();
            UpdateRows();
        }

        private void OnColumnWidthChanged(object sender, EventArgs e)
        {
            // При изменении ширины колонки обновляем максимальный горизонтальный offset.
            // Важно: не синхронизируем напрямую CellViewModel через WidthProperty
            // (кроме как через слабую ссылку, чтобы избежать утечек при Dispose).
            UpdateMaxHorizontalOffset();
        }

        private static void OnPropertyChangedThrottled(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var presenter = (UnifiedRowsPresenter)d;

            if (e.Property == LeftFrozenColumnsCountProperty || e.Property == RightFrozenColumnsCountProperty)
            {
                presenter.UpdateMaxHorizontalOffset();
                // UpdateRows() НЕ вызываем здесь — он вызывается вручную из SyncColumnsWithHeaders()
                // после установки обоих счетчиков LeftFrozenColumnsCount и RightFrozenColumnsCount.
                // Вызов UpdateRows() здесь приводит к двойному/тройному пересозданию строк
                // при синхронизации колонок после Drag&Drop.
            }
        }

        private static void OnVerticalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var presenter = (UnifiedRowsPresenter)d;
            var newOffset = (double)e.NewValue;

            if (presenter._isUpdatingScroll) return;

            presenter._isUpdatingScroll = true;
            try
            {
                // Синхронизируем ItemsControl через ScrollViewer с новым смещением
                if (presenter._itemsControl != null)
                {
                    presenter._itemsControl.ScrollToVerticalOffset(newOffset);
                }
            }
            finally
            {
                presenter._isUpdatingScroll = false;
            }
        }

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var presenter = (UnifiedRowsPresenter)d;
            System.Diagnostics.Debug.WriteLine($"[UnifiedRowsPresenter] OnItemsSourceChanged: old={e.OldValue?.GetType().Name ?? "null"}, new={e.NewValue?.GetType().Name ?? "null"}, IsLoaded={presenter.IsLoaded}");
            presenter.UpdateRows();
        }

        public void UpdateRows()
        {
            System.Diagnostics.Debug.WriteLine($"[UnifiedRowsPresenter] UpdateRows: IsLoaded={IsLoaded}, ItemsSource={ItemsSource?.GetType().Name ?? "null"}");

            if (IsLoaded)
            {
                // При смене ItemsSource принудительно очищаем все контейнеры,
                // чтобы гарантировать вызов Dispose() для всех старых RowViewModel.
                // WPF сам не вызывает ClearContainerForItemOverride для всех контейнеров
                // при смене ItemsSource, особенно при использовании VirtualizationMode.Recycling.
                ForceCleanAllContainers();

                _itemsControl.ItemsSource = ItemsSource;

                // Принудительно вызываем measure и visual, чтобы гарантировать
                // корректное отображение после смены ItemsSource на null
                // (или на новую коллекцию). Иначе WPF может не обновить визуальное
                // представление и оставить старые строки на экране.
                _itemsControl.InvalidateMeasure();
                _itemsControl.InvalidateVisual();
                InvalidateVisual();
                LogVirtualizationStats();
            }
        }

        /// <summary>
        /// Принудительно очищает все контейнеры строк, минуя стандартный
        /// ClearContainerForItemOverride. Гарантирует, что каждый RowViewModel
        /// получит Dispose() при смене ItemsSource.
        /// После очистки устанавливает ItemsSource в null,
        /// чтобы ItemContainerGenerator не держал ссылки на старые контейнеры.
        /// </summary>
        private void ForceCleanAllContainers()
        {
            if (_itemsControl == null) return;

            try
            {
                var itemsCount = _itemsControl.Items.Count;
                for (int i = 0; i < itemsCount; i++)
                {
                    var container = _itemsControl.ItemContainerGenerator.ContainerFromIndex(i) as RowContainer;
                    if (container != null)
                    {
                        // Clear() сбрасывает DataContext в PreviousViewModel,
                        // вызывает Dispose() у RowViewModel, отписывает PreviewMouseDown,
                        // DataContextChanged, очищает DataContext, Content, ContentTemplate
                        // и остальные ресурсы. Гарантирует чистую перезагрузку строк.
                        container.Clear();
                    }
                }

                // Устанавливаем ItemsSource в null, чтобы ItemContainerGenerator
                // не держал ссылки на старые контейнеры через ItemContainerGenerator.
                // Это также заставляет WPF освободить внутренние ресурсы,
                // что критично при последующей установке нового ItemsSource.
                _itemsControl.ItemsSource = null;
            }
            catch
            {
                // Игнорируем ошибки при очистке
            }
        }

        public void ScrollToVerticalOffset(double offset)
        {
            _itemsControl?.ScrollToVerticalOffset(offset);
        }

        /// <summary>
        /// Возвращает только видимые колонки (исключая скрытые из viewport).
        /// Используется для фильтрации колонок через TranslateTransform и шаблоны строк
        /// (ScrollBehavior), которые скрывают колонки за пределами видимости.
        /// Для CellViewModel создаются только для видимых колонок, остальные игнорируются.
        /// </summary>
        public List<DataGridColumn> GetVisibleCenterColumns()
        {
            if (_parentGrid == null) return new List<DataGridColumn>();

            var columns = _parentGrid.Columns;
            int leftCount = LeftFrozenColumnsCount;
            int rightCount = RightFrozenColumnsCount;
            int centerStart = leftCount;
            int centerEnd = columns.Count - rightCount;

            if (centerStart >= centerEnd)
                return new List<DataGridColumn>();

            var result = new List<DataGridColumn>(centerEnd - centerStart);
            for (int i = centerStart; i < centerEnd; i++)
            {
                var column = columns[i];
                var headerItem = _parentGrid.GetColumnHeaderItem(column);
                if (headerItem == null || headerItem.IsVisible)
                {
                    result.Add(column);
                }
            }

            return result;
        }

        /// <summary>
        /// Обновляет CellViewModel во всех активных строках при изменении колонок.
        /// Используется для обновления при изменении видимости колонок (Drag&Drop, hide/show).
        /// В отличие от UpdateRows(), не пересоздает строки и не меняет ItemsSource,
        /// обновляя только коллекции ячеек в каждой строке.
        /// </summary>
        internal void RefreshRowCells()
        {
            if (_itemsControl == null || _parentGrid == null) return;

            var columns = _parentGrid.Columns;
            int leftCount = LeftFrozenColumnsCount;
            int rightCount = RightFrozenColumnsCount;

            // Формируем списки колонок для каждой панели (если они еще не заданы)
            var leftColumns = columns.Take(leftCount)
                .Where(col => IsColumnVisible(col))
                .ToList();
            var centerColumns = GetVisibleCenterColumns();
            var rightColumns = columns.Skip(columns.Count - rightCount)
                .Where(col => IsColumnVisible(col))
                .ToList();

            // Обновляем ячейки во всех активных строках (синхронный обход)
            for (int i = 0; i < _itemsControl.Items.Count; i++)
            {
                var container = _itemsControl.ItemContainerGenerator.ContainerFromIndex(i) as RowContainer;
                if (container?.DataContext is RowViewModel vm)
                {
                    vm.UpdateCells(leftColumns, centerColumns, rightColumns);
                }
            }
        }

        /// <summary>
        /// Проверяет, видна ли колонка (через ColumnHeaderItem.IsVisible).
        /// </summary>
        private bool IsColumnVisible(DataGridColumn col)
        {
            var headerItem = _parentGrid?.GetColumnHeaderItem(col);
            return headerItem == null || headerItem.IsVisible;
        }

        private void LogVirtualizationStats()
        {
            if (_itemsControl == null) return;

            var now = DateTime.Now;
            if ((now - _lastStatsLog).TotalMilliseconds < 1000) return;
            _lastStatsLog = now;

            int realizedCount = 0;
            int totalItems = ItemsSource?.Cast<object>().Count() ?? 0;

            try
            {
                var itemsCount = _itemsControl.Items.Count;
                for (int i = 0; i < itemsCount; i++)
                {
                    var container = _itemsControl.ItemContainerGenerator.ContainerFromIndex(i);
                    if (container != null)
                    {
                        realizedCount++;
                    }
                }
            }
            catch
            {
                // Игнорируем ошибки
            }

            ScrollDiagnostics.LogVirtualizationStats(0, realizedCount, totalItems, 0);

            System.Diagnostics.Debug.WriteLine($"?? Virtualization Stats - Realized: {realizedCount}, Total: {totalItems}, Ratio: {(totalItems > 0 ? (realizedCount * 100.0 / totalItems).ToString("F1") : "N/A")}%");
        }
    }
}

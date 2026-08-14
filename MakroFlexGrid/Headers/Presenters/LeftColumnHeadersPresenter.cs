using MakroFlexGrid.Core;
using MakroFlexGrid.Rows;
using MakroFlexGrid.Utilities;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MakroFlexGrid.Headers
{
    public class LeftColumnHeadersPresenter : StackPanel
    {
        public static readonly DependencyProperty LeftFrozenColumnsCountProperty =
            DependencyProperty.Register(nameof(LeftFrozenColumnsCount), typeof(int),
            typeof(LeftColumnHeadersPresenter),
            new PropertyMetadata(0, OnLeftFrozenColumnsCountChanged));

        public static readonly DependencyProperty RightFrozenColumnsCountProperty =
            DependencyProperty.Register(nameof(RightFrozenColumnsCount), typeof(int),
            typeof(LeftColumnHeadersPresenter),
            new PropertyMetadata(0, OnRightFrozenColumnsCountChanged));

        public static readonly DependencyProperty ScrollableWidthProperty =
            DependencyProperty.Register(nameof(ScrollableWidth), typeof(double),
            typeof(LeftColumnHeadersPresenter),
            new PropertyMetadata(0.0));

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

        public double ScrollableWidth
        {
            get => (double)GetValue(ScrollableWidthProperty);
            private set => SetValue(ScrollableWidthProperty, value);
        }

        private CustomDataGrid _parentGrid;
        private UnifiedRowsPresenter _presenter;
        private double _totalWidth = 0;
        private bool _isSubscribedToScroll = false;
        private bool _isUpdatePending = false;
        private readonly StackPanel _contentPanel;

        // Слабая подписка на ScrollManager — позволяет LeftColumnHeadersPresenter быть собранным GC
        private Action<double> _weakScrollHandler;

        // WeakDependencyPropertyListener для автоматической отписки при сборке GC
        private readonly List<WeakDependencyPropertyListener> _columnWidthListeners = new List<WeakDependencyPropertyListener>();

        public LeftColumnHeadersPresenter()
        {
            Orientation = Orientation.Horizontal;
            Margin = new Thickness(0);
            VerticalAlignment = VerticalAlignment.Top;

            // Включаем обрезку содержимого, выходящего за границы
            ClipToBounds = true;

            // Внутренняя панель для контента — TranslateTransform применяется к ней,
            // а не ко всему StackPanel, чтобы не смещать сам контейнер за границы ClipToBounds
            _contentPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };
            Children.Add(_contentPanel);

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _parentGrid = FindParent<CustomDataGrid>(this);
            if (_parentGrid != null)
            {
                _parentGrid.Columns.CollectionChanged += OnColumnsCollectionChanged;
                _parentGrid.SizeChanged += OnParentGridSizeChanged;

                // Подписываемся на изменение ширины колонок через WeakDependencyPropertyListener
                SubscribeToColumnWidths();

                // Находим UnifiedRowsPresenter и подписываемся на ScrollManager через слабую ссылку
                FindAndSubscribeToScrollManager();

                UpdatePanel();
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            UnsubscribeFromColumnWidths();
            UnsubscribeFromScrollManager();

            if (_parentGrid != null)
            {
                _parentGrid.Columns.CollectionChanged -= OnColumnsCollectionChanged;
                _parentGrid.SizeChanged -= OnParentGridSizeChanged;
            }
        }

        private void OnColumnsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // При изменении коллекции колонок переподписываемся
            UnsubscribeFromColumnWidths();
            SubscribeToColumnWidths();
            UpdatePanel();
        }

        private void OnParentGridSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdatePanel();
        }

        /// <summary>
        /// Подписывается на изменение ActualWidth всех колонок через WeakDependencyPropertyListener.
        /// Автоматически отписывается при сборке GC подписчика.
        /// </summary>
        private void SubscribeToColumnWidths()
        {
            if (_parentGrid == null) return;

            foreach (DataGridColumn column in _parentGrid.Columns)
            {
                var descriptor = DependencyPropertyDescriptor.FromProperty(
                    DataGridColumn.ActualWidthProperty, typeof(DataGridColumn));
                if (descriptor != null)
                {
                    var listener = new WeakDependencyPropertyListener(
                        descriptor, column, this, OnColumnWidthChanged);
                    _columnWidthListeners.Add(listener);
                }
            }
        }

        /// <summary>
        /// Отписывается от изменения ActualWidth всех колонок.
        /// </summary>
        private void UnsubscribeFromColumnWidths()
        {
            foreach (var listener in _columnWidthListeners)
            {
                listener.Dispose();
            }
            _columnWidthListeners.Clear();
        }

        private void OnColumnWidthChanged(object sender, EventArgs e)
        {
            UpdatePanel();
        }

        private void FindAndSubscribeToScrollManager()
        {
            if (_parentGrid == null) return;

            // Ищем UnifiedRowsPresenter через визуальное дерево от _parentGrid
            _presenter = FindVisualChild<UnifiedRowsPresenter>(_parentGrid);
            if (_presenter?.ScrollManager != null && !_isSubscribedToScroll)
            {
                // Используем слабую подписку через WeakActionHelper с auto-unsubscribe,
                // чтобы LeftColumnHeadersPresenter мог быть собран GC,
                // даже если Dispose() не был вызван.
                // Перегрузка с unsubscribeAction гарантирует автоматическую отписку
                // при сборке GC подписчика, предотвращая накопление мёртвых делегатов
                // в ScrollManager.HorizontalOffsetChanged.
                _weakScrollHandler = WeakActionHelper.CreateWeakAction<double>(
                    this,
                    OnScrollManagerOffsetChanged,
                    handler => _presenter.ScrollManager.HorizontalOffsetChanged -= handler);
                _presenter.ScrollManager.HorizontalOffsetChanged += _weakScrollHandler;
                _isSubscribedToScroll = true;

                // Устанавливаем начальный offset
                ApplyHorizontalOffset(_presenter.ScrollManager.HorizontalOffset);
            }
        }

        private void UnsubscribeFromScrollManager()
        {
            if (_presenter?.ScrollManager != null && _weakScrollHandler != null)
            {
                _presenter.ScrollManager.HorizontalOffsetChanged -= _weakScrollHandler;
            }
            _weakScrollHandler = null;
            _presenter = null;
            _isSubscribedToScroll = false;
        }

        private void OnScrollManagerOffsetChanged(double offset)
        {
            // Схлопываем множественные вызовы при интенсивном скроллинге:
            // если уже есть отложенная операция, не создаём новую.
            // Это предотвращает накопление DispatcherOperation в очереди диспетчера.
            if (_isUpdatePending) return;
            _isUpdatePending = true;

            // Используем BeginInvoke с приоритетом Render для минимизации визуальных лагов.
            // BeginInvoke (в отличие от Invoke) не блокирует вызывающий поток и позволяет
            // очереди диспетчера схлопывать повторяющиеся операции при интенсивном скроллинге.
            Dispatcher.BeginInvoke(new Action(() =>
            {
                _isUpdatePending = false;
                ApplyHorizontalOffset(offset);
            }), System.Windows.Threading.DispatcherPriority.Render);
        }

        private void ApplyHorizontalOffset(double offset)
        {
            // Применяем TranslateTransform к внутреннему контейнеру _contentPanel,
            // а не ко всему StackPanel. Это гарантирует, что сам LeftColumnHeadersPresenter
            // остаётся на месте, и ClipToBounds на родительском Border корректно обрезает
            // только контент, выходящий за границы центральной колонки.
            if (_contentPanel.RenderTransform == null || !(_contentPanel.RenderTransform is TranslateTransform))
            {
                _contentPanel.RenderTransform = new TranslateTransform();
            }

            var transform = (TranslateTransform)_contentPanel.RenderTransform;
            transform.X = -offset;
        }

        private static T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child is T t) return t;
                T childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null) return childOfChild;
            }
            return null;
        }

        private static void OnLeftFrozenColumnsCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((LeftColumnHeadersPresenter)d).UpdatePanel();
        }

        private static void OnRightFrozenColumnsCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((LeftColumnHeadersPresenter)d).UpdatePanel();
        }

        private void UpdatePanel()
        {
            // Схлопываем множественные вызовы: если уже есть отложенная операция, не создаём новую.
            // Это предотвращает накопление DispatcherOperation в очереди при интенсивном скроллинге.
            if (_isUpdatePending) return;
            _isUpdatePending = true;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                _isUpdatePending = false;
                _contentPanel.Children.Clear();
                if (_parentGrid == null || _parentGrid.Columns.Count == 0) return;

                int leftCount = LeftFrozenColumnsCount;
                int rightCount = RightFrozenColumnsCount;
                int totalColumns = _parentGrid.Columns.Count;
                int startIndex = leftCount;
                int endIndex = Math.Max(startIndex, totalColumns - rightCount);

                _totalWidth = 0;
                for (int i = startIndex; i < endIndex; i++)
                {
                    var column = _parentGrid.Columns[i];
                    double columnWidth = column.ActualWidth > 0 ? column.ActualWidth : 100;
                    _totalWidth += columnWidth;

                    var border = new Border
                    {
                        BorderBrush = _parentGrid?.GridLineBrush ?? Brushes.LightGray,
                        BorderThickness = new Thickness(0, 0, 1, 1),
                        Background = _parentGrid?.CenterPanelBackground ?? Brushes.Transparent,
                        Width = columnWidth,
                        Height = 30,
                        SnapsToDevicePixels = true
                    };

                    if (column.Header != null)
                    {
                        border.Child = new TextBlock
                        {
                            Text = column.Header.ToString(),
                            Margin = new Thickness(6, 4, 6, 4),
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                            FontWeight = FontWeights.Bold
                        };
                    }

                    _contentPanel.Children.Add(border);
                }

                // Вычисляем ScrollableWidth на основе ширины самого презентера
                double availableWidth = ActualWidth;
                ScrollableWidth = Math.Max(0, _totalWidth - availableWidth);

                // После перестроения содержимого применяем текущий offset
                if (_presenter?.ScrollManager != null)
                {
                    ApplyHorizontalOffset(_presenter.ScrollManager.HorizontalOffset);
                }

                // Принудительно перерисовываем для применения обрезки
                InvalidateVisual();
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        private static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            while (true)
            {
                if (child == null) return null;
                var parent = VisualTreeHelper.GetParent(child);
                if (parent == null) return null;
                if (parent is T typedParent) return typedParent;
                child = parent;
            }
        }

        // Дополнительная защита: переопределяем Arrange для гарантии обрезки
        protected override Size ArrangeOverride(Size arrangeSize)
        {
            var result = base.ArrangeOverride(arrangeSize);

            // Убеждаемся, что ClipToBounds активен
            if (!ClipToBounds)
                ClipToBounds = true;

            return result;
        }
    }
}

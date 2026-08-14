using MakroFlexGrid.Core;
using MakroFlexGrid.Utilities;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MakroFlexGrid.Headers
{
    public class RightColumnHeadersPresenter : StackPanel
    {
        public static readonly DependencyProperty RightFrozenColumnsCountProperty =
            DependencyProperty.Register(nameof(RightFrozenColumnsCount), typeof(int),
            typeof(RightColumnHeadersPresenter),
            new PropertyMetadata(0, OnRightFrozenColumnsCountChanged));

        public static readonly DependencyProperty FrozenPanelBackgroundProperty =
            DependencyProperty.Register(nameof(FrozenPanelBackground), typeof(Brush),
            typeof(RightColumnHeadersPresenter),
            new PropertyMetadata(Brushes.Transparent, OnFrozenPanelBackgroundChanged));

        public int RightFrozenColumnsCount
        {
            get => (int)GetValue(RightFrozenColumnsCountProperty);
            set => SetValue(RightFrozenColumnsCountProperty, value);
        }

        public Brush FrozenPanelBackground
        {
            get => (Brush)GetValue(FrozenPanelBackgroundProperty);
            set => SetValue(FrozenPanelBackgroundProperty, value);
        }

        private CustomDataGrid _parentGrid;

        // WeakDependencyPropertyListener для автоматической отписки при сборке GC
        private WeakDependencyPropertyListener _rightMarginListener;
        private readonly List<WeakDependencyPropertyListener> _columnWidthListeners = new List<WeakDependencyPropertyListener>();
        private bool _isUpdatePending = false;

        public RightColumnHeadersPresenter()
        {
            Orientation = Orientation.Horizontal;
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            FindAndAttachToGrid();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            DetachFromGrid();
        }

        private void FindAndAttachToGrid()
        {
            _parentGrid = FindParent<CustomDataGrid>(this);
            if (_parentGrid != null)
            {
                AttachToGrid();
                UpdatePanel();
            }
        }

        private void AttachToGrid()
        {
            if (_parentGrid == null) return;

            _parentGrid.Columns.CollectionChanged += OnColumnsChanged;

            // Подписываемся на изменение RightMargin через WeakDependencyPropertyListener
            var marginDescriptor = DependencyPropertyDescriptor.FromProperty(
                CustomDataGrid.RightMarginProperty, typeof(CustomDataGrid));
            _rightMarginListener = new WeakDependencyPropertyListener(
                marginDescriptor, _parentGrid, this, OnRightMarginChanged);

            foreach (DataGridColumn column in _parentGrid.Columns)
            {
                SubscribeToColumnWidth(column);
            }
        }

        private void SubscribeToColumnWidth(DataGridColumn column)
        {
            var descriptor = DependencyPropertyDescriptor.FromProperty(
                DataGridColumn.ActualWidthProperty, typeof(DataGridColumn));
            var listener = new WeakDependencyPropertyListener(
                descriptor, column, this, OnColumnWidthChanged);
            _columnWidthListeners.Add(listener);
        }

        private void DetachFromGrid()
        {
            if (_parentGrid == null) return;

            _parentGrid.Columns.CollectionChanged -= OnColumnsChanged;

            // WeakDependencyPropertyListener сам отпишется при Dispose или при сборке GC
            if (_rightMarginListener != null)
            {
                _rightMarginListener.Dispose();
                _rightMarginListener = null;
            }

            foreach (var listener in _columnWidthListeners)
            {
                listener.Dispose();
            }
            _columnWidthListeners.Clear();

            _parentGrid = null;
        }

        private void OnRightMarginChanged(object sender, EventArgs e)
        {
            UpdatePanel();
        }

        private void OnColumnsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (DataGridColumn column in e.NewItems)
                {
                    SubscribeToColumnWidth(column);
                }
            }

            if (e.OldItems != null)
            {
                foreach (DataGridColumn column in e.OldItems)
                {
                    // Ищем и удаляем listener для этой колонки
                    var listener = _columnWidthListeners.Find(l => l.Source == column);
                    if (listener != null)
                    {
                        listener.Dispose();
                        _columnWidthListeners.Remove(listener);
                    }
                }
            }

            UpdatePanel();
        }

        private void OnColumnWidthChanged(object sender, EventArgs e)
        {
            UpdatePanel();
        }

        private static void OnRightFrozenColumnsCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((RightColumnHeadersPresenter)d).UpdatePanel();
        }

        private static void OnFrozenPanelBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((RightColumnHeadersPresenter)d).UpdatePanel();
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
                Children.Clear();
                var grid = _parentGrid ?? FindParent<CustomDataGrid>(this);
                if (grid == null || grid.Columns.Count == 0) return;

                int rightCount = RightFrozenColumnsCount;
                int totalColumns = grid.Columns.Count;
                int startIndex = Math.Max(0, totalColumns - rightCount);

                for (int i = startIndex; i < totalColumns; i++)
                {
                    var column = grid.Columns[i];
                    double columnWidth = column.ActualWidth > 0 ? column.ActualWidth : 100;

                    var border = new Border
                    {
                        BorderBrush = grid?.GridLineBrush ?? Brushes.LightGray,
                        BorderThickness = new Thickness(0, 0, 1, 1),
                        Background = FrozenPanelBackground,
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

                    Children.Add(border);
                }
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
    }
}

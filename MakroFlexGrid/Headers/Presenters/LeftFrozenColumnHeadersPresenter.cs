using MakroFlexGrid.Core;
using MakroFlexGrid.Utilities;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MakroFlexGrid.Headers
{
    public class LeftFrozenColumnHeadersPresenter : StackPanel
    {
        public static readonly DependencyProperty LeftFrozenColumnsCountProperty =
            DependencyProperty.Register(nameof(LeftFrozenColumnsCount), typeof(int),
            typeof(LeftFrozenColumnHeadersPresenter),
            new PropertyMetadata(0, OnLeftFrozenColumnsCountChanged));

        public static readonly DependencyProperty FrozenPanelBackgroundProperty =
            DependencyProperty.Register(nameof(FrozenPanelBackground), typeof(Brush),
            typeof(LeftFrozenColumnHeadersPresenter),
            new PropertyMetadata(Brushes.Transparent, OnFrozenPanelBackgroundChanged));

        public int LeftFrozenColumnsCount
        {
            get => (int)GetValue(LeftFrozenColumnsCountProperty);
            set => SetValue(LeftFrozenColumnsCountProperty, value);
        }

        public Brush FrozenPanelBackground
        {
            get => (Brush)GetValue(FrozenPanelBackgroundProperty);
            set => SetValue(FrozenPanelBackgroundProperty, value);
        }

        private CustomDataGrid _parentGrid;

        // WeakDependencyPropertyListener для автоматической отписки при сборке GC
        private WeakDependencyPropertyListener _leftMarginListener;
        private readonly List<WeakDependencyPropertyListener> _columnWidthListeners = new List<WeakDependencyPropertyListener>();
        private bool _isUpdatePending = false;

        public LeftFrozenColumnHeadersPresenter()
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

            // Подписываемся на изменение LeftMargin через WeakDependencyPropertyListener
            var marginDescriptor = DependencyPropertyDescriptor.FromProperty(
                CustomDataGrid.LeftMarginProperty, typeof(CustomDataGrid));
            _leftMarginListener = new WeakDependencyPropertyListener(
                marginDescriptor, _parentGrid, this, OnLeftMarginChanged);

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
            if (_leftMarginListener != null)
            {
                _leftMarginListener.Dispose();
                _leftMarginListener = null;
            }

            foreach (var listener in _columnWidthListeners)
            {
                listener.Dispose();
            }
            _columnWidthListeners.Clear();

            _parentGrid = null;
        }

        private void OnLeftMarginChanged(object sender, EventArgs e)
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

        private static void OnLeftFrozenColumnsCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((LeftFrozenColumnHeadersPresenter)d).UpdatePanel();
        }

        private static void OnFrozenPanelBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((LeftFrozenColumnHeadersPresenter)d).UpdatePanel();
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

                int leftCount = LeftFrozenColumnsCount;
                int totalColumns = grid.Columns.Count;
                int endIndex = Math.Min(leftCount, totalColumns);
                double leftPadding = grid.LeftMargin.Left;

                // Добавляем отступ для компенсации скроллбара слева
                if (leftPadding > 0)
                {
                    var paddingBorder = new Border
                    {
                        Width = leftPadding,
                        Height = 30,
                        Background = FrozenPanelBackground,
                        BorderBrush = grid?.GridLineBrush ?? Brushes.Gray,
                        BorderThickness = new Thickness(0, 0, 0, 1)
                    };
                    Children.Add(paddingBorder);
                }

                for (int i = 0; i < endIndex; i++)
                {
                    var column = grid.Columns[i];
                    double columnWidth = column.ActualWidth > 0 ? column.ActualWidth : 100;

                    var border = new Border
                    {
                        BorderBrush = grid?.GridLineBrush ?? Brushes.Gray,
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

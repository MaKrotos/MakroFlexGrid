using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using MakroFlexGrid.Core;

namespace MakroFlexGrid.Rows
{
    public class UnifiedRowsItemsControl : ItemsControl
    {
        private readonly UnifiedRowsPresenter _parentPresenter;
        private ScrollViewer _scrollViewer;
        private VirtualizingStackPanel _virtualizingStackPanel;

        /// <summary>
        /// Событие изменения вертикального скролла.
        /// </summary>
        public event EventHandler<ScrollChangedEventArgs> VerticalScrollChanged;

        /// <summary>
        /// Текущий вертикальный offset.
        /// </summary>
        public double VerticalOffset => _scrollViewer?.VerticalOffset ?? 0;

        /// <summary>
        /// Видимость вертикального скроллбара.
        /// </summary>
        public Visibility ComputedVerticalScrollBarVisibility =>
            _scrollViewer?.ComputedVerticalScrollBarVisibility ?? Visibility.Collapsed;

        /// <summary>
        /// Прокручивает к указанной вертикальной позиции.
        /// </summary>
        public void ScrollToVerticalOffset(double offset)
        {
            _scrollViewer?.ScrollToVerticalOffset(offset);
        }

        /// <summary>
        /// Прокручивает к указанной горизонтальной позиции.
        /// </summary>
        public void ScrollToHorizontalOffset(double offset)
        {
            _scrollViewer?.ScrollToHorizontalOffset(offset);
        }

        static UnifiedRowsItemsControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(UnifiedRowsItemsControl),
                new FrameworkPropertyMetadata(typeof(UnifiedRowsItemsControl)));
        }

        public UnifiedRowsItemsControl(UnifiedRowsPresenter parentPresenter)
        {
            _parentPresenter = parentPresenter ?? throw new ArgumentNullException(nameof(parentPresenter));

            // Загружаем шаблон из Themes/RowTemplates.xaml в текущей сборке
            var rowTemplate = LoadRowTemplate();
            if (rowTemplate != null)
            {
                ItemTemplate = rowTemplate;
            }

            System.Diagnostics.Debug.WriteLine($"[UnifiedRowsItemsControl] Constructor: DefaultStyleKey={DefaultStyleKey}");
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _scrollViewer = GetTemplateChild("PART_ScrollViewer") as ScrollViewer;

            System.Diagnostics.Debug.WriteLine($"[UnifiedRowsItemsControl] OnApplyTemplate: _scrollViewer={_scrollViewer != null}");

            if (_scrollViewer != null)
            {
                // Отписываем предыдущий обработчик перед подпиской (защита от повторного OnApplyTemplate)
                _scrollViewer.ScrollChanged -= OnScrollChanged;
                _scrollViewer.ScrollChanged += OnScrollChanged;
            }

            // VirtualizingStackPanel создаётся ItemsPresenter'ом ПОСЛЕ применения шаблона,
            // поэтому в OnApplyTemplate его ещё нет в визуальном дереве.
            // Используем Dispatcher.BeginInvoke с приоритетом Loaded, чтобы найти панель
            // после того, как ItemsControl полностью отрисован.
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(TrySubscribeToCleanUpVirtualizedItem));
        }

        /// <summary>
        /// Пытается найти VirtualizingStackPanel в визуальном дереве и подписаться
        /// на CleanUpVirtualizedItem. Вызывается отложенно через Dispatcher,
        /// чтобы ItemsPresenter успел создать ItemsPanel.
        /// </summary>
        private void TrySubscribeToCleanUpVirtualizedItem()
        {
            // Отписываем предыдущий обработчик (защита от повторного вызова)
            if (_virtualizingStackPanel != null)
            {
                _virtualizingStackPanel.RemoveHandler(VirtualizingStackPanel.CleanUpVirtualizedItemEvent, new CleanUpVirtualizedItemEventHandler(OnCleanUpVirtualizedItem));
                _virtualizingStackPanel = null;
            }

            // Ищем VirtualizingStackPanel через визуальное дерево
            _virtualizingStackPanel = FindVisualChild<VirtualizingStackPanel>(this);
            if (_virtualizingStackPanel != null)
            {
                _virtualizingStackPanel.AddHandler(VirtualizingStackPanel.CleanUpVirtualizedItemEvent, new CleanUpVirtualizedItemEventHandler(OnCleanUpVirtualizedItem));
                System.Diagnostics.Debug.WriteLine($"[UnifiedRowsItemsControl] VirtualizingStackPanel found, CleanUpVirtualizedItem subscribed");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[UnifiedRowsItemsControl] VirtualizingStackPanel NOT FOUND at Loaded priority, will retry");
                // Если панель ещё не создана, повторяем попытку позже
                Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(TrySubscribeToCleanUpVirtualizedItem));
            }
        }

        /// <summary>
        /// Вызывается, когда элемент вытесняется из виртуализации VirtualizingStackPanel.
        /// Гарантирует вызов Dispose() для RowViewModel, даже если ClearContainerForItemOverride
        /// не был вызван (например, при быстром скролле с CacheLength > 0).
        /// </summary>
        private void OnCleanUpVirtualizedItem(object sender, CleanUpVirtualizedItemEventArgs e)
        {
            if (e.UIElement is RowContainer container)
            {
                // Clear() сам сохраняет DataContext в PreviousViewModel,
                // вызывает Dispose() для RowViewModel, отписывает PreviewMouseDown,
                // сбрасывает DataContext, Content, ContentTemplate и обработчики кликов.
                container.Clear();
                System.Diagnostics.Debug.WriteLine($"[UnifiedRowsItemsControl] CleanUpVirtualizedItem: container cleared");
            }
        }

        /// <summary>
        /// Ищет дочерний элемент заданного типа в визуальном дереве.
        /// </summary>
        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T found)
                    return found;

                var result = FindVisualChild<T>(child);
                if (result != null)
                    return result;
            }
            return null;
        }

        private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            VerticalScrollChanged?.Invoke(this, e);
        }

        private DataTemplate LoadRowTemplate()
        {
            try
            {
                // Путь к ресурсу в текущей сборке
                var uri = new Uri("/MakroFlexGrid;component/Themes/RowTemplates.xaml", UriKind.RelativeOrAbsolute);
                var resourceDictionary = new ResourceDictionary
                {
                    Source = uri
                };

                return resourceDictionary["RowTemplate"] as DataTemplate;
            }
            catch (Exception ex)
            {
                // Логируем ошибку, если нужно
                System.Diagnostics.Debug.WriteLine($"Failed to load RowTemplate: {ex.Message}");
                return null;
            }
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new RowContainer();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is RowContainer;
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            if (!(element is RowContainer rowContainer))
            {
                base.PrepareContainerForItemOverride(element, item);
                return;
            }

            var oldVm = rowContainer.DataContext as RowViewModel;
            rowContainer.Clear(forceVm: oldVm);
            rowContainer.EnsureSubscribed();

            var rowVm = new RowViewModel(item, _parentPresenter);
            if (_parentPresenter.ParentGrid != null)
            {
                var grid = _parentPresenter.ParentGrid;
                var columns = grid.Columns;
                int leftCount = _parentPresenter.LeftFrozenColumnsCount;
                int rightCount = _parentPresenter.RightFrozenColumnsCount;

                // Фильтруем левые колонки по видимости
                var leftColumns = columns.Take(leftCount)
                    .Where(col =>
                    {
                        var headerItem = grid.GetColumnHeaderItem(col);
                        return headerItem == null || headerItem.IsVisible;
                    })
                    .ToList();

                var visibleCenterColumns = _parentPresenter.GetVisibleCenterColumns();

                // Фильтруем правые колонки по видимости
                var rightColumns = columns.Skip(columns.Count - rightCount)
                    .Where(col =>
                    {
                        var headerItem = grid.GetColumnHeaderItem(col);
                        return headerItem == null || headerItem.IsVisible;
                    })
                    .ToList();

                rowVm.UpdateCells(leftColumns, visibleCenterColumns, rightColumns);

                if (grid.RowSelectionMode == RowSelectionMode.Multiple)
                {
                    rowVm.IsSelected = grid.SelectedItems.Contains(item);
                }
                else if (grid.RowSelectionMode == RowSelectionMode.Single)
                {
                    rowVm.IsSelected = ReferenceEquals(item, grid.SelectedItem);
                }
                else // None
                {
                    rowVm.IsSelected = false;
                }
            }

            rowContainer.DataContext = rowVm;
            rowContainer.Content = rowVm;
            rowContainer.ContentTemplate = ItemTemplate;

            rowContainer.SetRowClickedHandler((vm) =>
            {
                _parentPresenter.ParentGrid?.OnRowClicked(vm);
            });

            rowContainer.SetRowDoubleClickedHandler((vm) =>
            {
                _parentPresenter.ParentGrid?.OnRowDoubleClicked(vm);
            });

            rowContainer.SetCellRightClickedHandler((vm, headerItem) =>
            {
                _parentPresenter.ParentGrid?.OnCellRightClicked(vm, headerItem);
            });

            rowContainer.SetCellClickedHandler((cellVm) =>
            {
                _parentPresenter.ParentGrid?.OnCellClicked(cellVm);
            });

            base.PrepareContainerForItemOverride(element, item);
        }


        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            if (!(element is RowContainer container))
            {
                base.ClearContainerForItemOverride(element, item);
                return;
            }

            // WPF сбрасывает DataContext в null ДО вызова ClearContainerForItemOverride,
            // поэтому container.DataContext уже недоступен. Используем параметр item,
            // который WPF передаёт как исходный DataContext контейнера.
            // Clear(forceVm) принимает VM извне для гарантированного вызова Dispose().
            container.Clear(item as RowViewModel);

            base.ClearContainerForItemOverride(element, item);
        }
    }
}

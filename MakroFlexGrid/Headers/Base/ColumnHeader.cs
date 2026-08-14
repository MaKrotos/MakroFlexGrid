using MakroFlexGrid.Core;
using MakroFlexGrid.Filters;
using MakroFlexGrid.Utilities;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace MakroFlexGrid.Headers
{
    /// <summary>
    /// Вспомогательный класс для хранения информации о пункте меню выбора агрегата.
    /// </summary>
    internal class AggregateMenuItemInfo
    {
        public Headers.AggregateType AggregateType { get; set; }
        public string DisplayName { get; set; }
    }

    /// <summary>
    /// Визуальный элемент заголовка колонки.
    /// ButtonBase с поддержкой Gripper для ресайза и отображения направления сортировки.
    /// Аналог BandHeader из FlexGrid.
    /// </summary>
    [TemplatePart(Name = RightHeaderGripperPartName, Type = typeof(ColumnHeaderGripper))]
    [TemplatePart(Name = LeftHeaderGripperPartName, Type = typeof(LeftColumnHeaderGripper))]
    public sealed class ColumnHeader : ButtonBase
    {
        static ColumnHeader()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ColumnHeader),
                new FrameworkPropertyMetadata(typeof(ColumnHeader)));
            WidthProperty.OverrideMetadata(
                typeof(ColumnHeader),
                new FrameworkPropertyMetadata(double.NaN));
        }

        internal ColumnHeader(ColumnHeaderItem ownerItem)
        {
            OwnerItem = ownerItem;
            Width = ownerItem.Width;
            Content = ownerItem.Header;
            ContentTemplate = ownerItem.HeaderTemplate;
            if (ownerItem.HeaderStyle != null)
                Style = ownerItem.HeaderStyle;
            HorizontalContentAlignment = ownerItem.HorizontalHeaderAlignment;
            VerticalContentAlignment = ownerItem.VerticalHeaderAlignment;
            HorizontalAlignment = HorizontalAlignment.Stretch;
            VerticalAlignment = VerticalAlignment.Stretch;

            // Подписываемся на изменения IsDragging и IsDropTarget у OwnerItem
            SubscribeToDragProperties();
        }

        /// <summary>
        /// Подписывается на изменения DependencyProperty IsDragging и IsDropTarget
        /// у OwnerItem (ColumnHeaderItem) и синхронизирует их с локальными DP.
        /// </summary>
        private void SubscribeToDragProperties()
        {
            if (OwnerItem == null) return;

            var isDraggingDescriptor = DependencyPropertyDescriptor.FromProperty(
                ColumnHeaderItem.IsDraggingProperty, typeof(ColumnHeaderItem));
            isDraggingDescriptor?.AddValueChanged(OwnerItem, (s, args) =>
            {
                IsDragging = OwnerItem.IsDragging;
            });

            var isDropTargetDescriptor = DependencyPropertyDescriptor.FromProperty(
                ColumnHeaderItem.IsDropTargetProperty, typeof(ColumnHeaderItem));
            isDropTargetDescriptor?.AddValueChanged(OwnerItem, (s, args) =>
            {
                IsDropTarget = OwnerItem.IsDropTarget;
            });
        }

        #region Constants

        public const string RightHeaderGripperPartName = "PART_RightHeaderGripper";
        public const string LeftHeaderGripperPartName = "PART_LeftHeaderGripper";

        #endregion

        #region Dependency Properties

        private static readonly DependencyProperty SortDirectionProperty =
            DependencyProperty.Register(
                "SortDirection",
                typeof(ListSortDirection?),
                typeof(ColumnHeader),
                new FrameworkPropertyMetadata(null, OnSortDirectionChanged));

        private static void OnSortDirectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Обновляем визуальное состояние для отображения стрелок сортировки
        }

        /// <summary>
        /// Определяет, находится ли данный заголовок в состоянии перетаскивания (Drag).
        /// Используется для XAML-триггера визуальной обратной связи.
        /// </summary>
        internal static readonly DependencyProperty IsDraggingProperty =
            DependencyProperty.Register(
                nameof(IsDragging),
                typeof(bool),
                typeof(ColumnHeader),
                new FrameworkPropertyMetadata(false));

        internal bool IsDragging
        {
            get => (bool)GetValue(IsDraggingProperty);
            set => SetValue(IsDraggingProperty, value);
        }

        /// <summary>
        /// Определяет, является ли данный заголовок целевой позицией вставки (Drop Target).
        /// Используется для XAML-триггера визуальной обратной связи.
        /// </summary>
        internal static readonly DependencyProperty IsDropTargetProperty =
            DependencyProperty.Register(
                nameof(IsDropTarget),
                typeof(bool),
                typeof(ColumnHeader),
                new FrameworkPropertyMetadata(false));

        internal bool IsDropTarget
        {
            get => (bool)GetValue(IsDropTargetProperty);
            set => SetValue(IsDropTargetProperty, value);
        }

        /// <summary>
        /// Определяет, активен ли фильтр на данной колонке.
        /// Используется для XAML-триггера визуальной индикации (иконка фильтра).
        /// </summary>
        internal static readonly DependencyProperty IsFilterActiveProperty =
            DependencyProperty.Register(
                nameof(IsFilterActive),
                typeof(bool),
                typeof(ColumnHeader),
                new FrameworkPropertyMetadata(false));

        internal bool IsFilterActive
        {
            get => (bool)GetValue(IsFilterActiveProperty);
            set => SetValue(IsFilterActiveProperty, value);
        }

        #endregion

        #region Private Variables

        private ColumnHeaderGripper _rightGripper;
        private LeftColumnHeaderGripper _leftGripper;
        private double _deferredWidth;
        private CustomDataGrid _parentGrid;

#if DEBUG
        private MakroFlexGrid.Headers.Debug.WidthTipPopup _widthTipPopup;
#endif

        #endregion

        #region Public Properties

        public ColumnHeaderItem OwnerItem { get; }

        public ListSortDirection? SortDirection
        {
            get => (ListSortDirection?)GetValue(SortDirectionProperty);
            set => SetValue(SortDirectionProperty, value);
        }

        #endregion

        #region Private Properties

        private CustomDataGrid ParentGrid
        {
            get
            {
                if (_parentGrid == null)
                {
                    if (OwnerItem?.OwnerGrid != null)
                        _parentGrid = OwnerItem.OwnerGrid;
                    else
                        _parentGrid = FindVisualParent<CustomDataGrid>(this);
                }
                return _parentGrid;
            }
        }

        private bool IsDeferredResizeEnabled
        {
            get
            {
                var grid = ParentGrid;
                return grid != null && grid.IsDeferredResizeEnabled;
            }
        }

        #endregion

        #region Private Methods

        private static T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
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

        private void AttachRightGripperEventHandlers(ColumnHeaderGripper gripper)
        {
            if (gripper != null)
            {
                DetachRightGripperEventHandlers(gripper);

                gripper.DragStarted += OnRightGripperDragStarted;
                gripper.DragDelta += OnRightGripperDragDelta;
                gripper.DragCompleted += OnRightGripperDragCompleted;
                gripper.MouseDoubleClick += OnRightGripperDoubleClicked;
            }
        }

        private void DetachRightGripperEventHandlers(ColumnHeaderGripper gripper)
        {
            gripper.DragStarted -= OnRightGripperDragStarted;
            gripper.DragDelta -= OnRightGripperDragDelta;
            gripper.DragCompleted -= OnRightGripperDragCompleted;
            gripper.MouseDoubleClick -= OnRightGripperDoubleClicked;
        }

        private void AttachLeftGripperEventHandlers(LeftColumnHeaderGripper gripper)
        {
            if (gripper != null)
            {
                DetachLeftGripperEventHandlers(gripper);

                gripper.DragStarted += OnLeftGripperDragStarted;
                gripper.DragDelta += OnLeftGripperDragDelta;
                gripper.DragCompleted += OnLeftGripperDragCompleted;
                gripper.MouseDoubleClick += OnLeftGripperDoubleClicked;
            }
        }

        private void DetachLeftGripperEventHandlers(LeftColumnHeaderGripper gripper)
        {
            gripper.DragStarted -= OnLeftGripperDragStarted;
            gripper.DragDelta -= OnLeftGripperDragDelta;
            gripper.DragCompleted -= OnLeftGripperDragCompleted;
            gripper.MouseDoubleClick -= OnLeftGripperDoubleClicked;
        }

        #endregion

        #region Private Event Handlers

        private void OnRightGripperDragStarted(object sender, DragStartedEventArgs e)
        {
            _deferredWidth = ActualWidth;
#if DEBUG
            if (Debugger.IsAttached)
                ShowWidthTip(_rightGripper, ActualWidth);
#endif
        }

        private void OnRightGripperDragDelta(object sender, DragDeltaEventArgs e)
        {
            if (OwnerItem == null) return;

            var newValue = Math.Max(OwnerItem.MinWidth, ActualWidth + e.HorizontalChange);

            // Ширина заголовка меняется сразу в любом режиме
            OwnerItem.Width = newValue;

#if DEBUG
            if (Debugger.IsAttached)
                UpdateWidthTip(newValue);
#endif

            if (IsDeferredResizeEnabled)
            {
                // В отложенном режиме запоминаем значение для синхронизации с колонкой при отпускании
                _deferredWidth = newValue;
            }
        }

        private void OnRightGripperDragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (OwnerItem != null && IsDeferredResizeEnabled)
            {
                // Принудительно синхронизируем ширину колонки DataGrid при отпускании
                OwnerItem.SetSyncColumnWidth(_deferredWidth);
            }
#if DEBUG
            if (Debugger.IsAttached)
                HideWidthTip();
#endif
        }

        private void OnRightGripperDoubleClicked(object sender, MouseButtonEventArgs e)
        {
            // TODO: Auto-fit width to content
            if (OwnerItem != null)
            {
                var rightItem = OwnerItem;
                while (rightItem.Children.Count > 0)
                    rightItem = rightItem.Children[rightItem.Children.Count - 1];

                // rightItem.Width = double.NaN;
            }
        }

        private void OnLeftGripperDragStarted(object sender, DragStartedEventArgs e)
        {
            _deferredWidth = ActualWidth;
#if DEBUG
            if (Debugger.IsAttached)
                ShowWidthTip(_leftGripper, ActualWidth);
#endif
        }

        private void OnLeftGripperDragDelta(object sender, DragDeltaEventArgs e)
        {
            if (OwnerItem == null) return;

            // При перетаскивании левого края вправо (e.HorizontalChange > 0),
            // ширина колонки должна УМЕНЬШАТЬСЯ.
            var newValue = Math.Max(OwnerItem.MinWidth, ActualWidth - e.HorizontalChange);

            // Ширина заголовка меняется сразу в любом режиме
            OwnerItem.Width = newValue;

#if DEBUG
            if (Debugger.IsAttached)
                UpdateWidthTip(newValue);
#endif

            if (IsDeferredResizeEnabled)
            {
                // В отложенном режиме запоминаем значение для синхронизации с колонкой при отпускании
                _deferredWidth = newValue;
            }
        }

        private void OnLeftGripperDragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (OwnerItem != null && IsDeferredResizeEnabled)
            {
                // Принудительно синхронизируем ширину колонки DataGrid при отпускании
                OwnerItem.SetSyncColumnWidth(_deferredWidth);
            }
#if DEBUG
            if (Debugger.IsAttached)
                HideWidthTip();
#endif
        }

        private void OnLeftGripperDoubleClicked(object sender, MouseButtonEventArgs e)
        {
            // TODO: Auto-fit width to content
        }

#if DEBUG
        private void ShowWidthTip(UIElement gripper, double width)
        {
            if (_widthTipPopup == null)
                _widthTipPopup = new MakroFlexGrid.Headers.Debug.WidthTipPopup();
            _widthTipPopup.Show(gripper, width);
        }

        private void UpdateWidthTip(double width)
        {
            _widthTipPopup?.UpdateWidth(width);
        }

        private void HideWidthTip()
        {
            _widthTipPopup?.Hide();
        }
#endif

        #endregion

        #region Public Override Methods

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _rightGripper = GetTemplateChild(RightHeaderGripperPartName) as ColumnHeaderGripper;
            _leftGripper = GetTemplateChild(LeftHeaderGripperPartName) as LeftColumnHeaderGripper;

            UpdateGripperVisibility();
        }

        internal void UpdateGripperVisibility()
        {
            if (OwnerItem == null) return;

            // Системные заголовки (SystemColumnHeaderItem, SystemLeafHeaderItem) не имеют Gripper
            if (OwnerItem is SystemColumnHeaderItem || OwnerItem is SystemLeafHeaderItem)
            {
                if (_rightGripper != null)
                {
                    _rightGripper.Visibility = Visibility.Collapsed;
                    DetachRightGripperEventHandlers(_rightGripper);
                }
                if (_leftGripper != null)
                {
                    _leftGripper.Visibility = Visibility.Collapsed;
                    DetachLeftGripperEventHandlers(_leftGripper);
                }
                return;
            }

            bool isLeft = OwnerItem.GripperPosition == GripperPositionType.Left;

            if (_rightGripper != null)
            {
                _rightGripper.Visibility = isLeft ? Visibility.Collapsed : Visibility.Visible;
                if (!isLeft) AttachRightGripperEventHandlers(_rightGripper);
                else DetachRightGripperEventHandlers(_rightGripper);
            }

            if (_leftGripper != null)
            {
                _leftGripper.Visibility = isLeft ? Visibility.Visible : Visibility.Collapsed;
                if (isLeft) AttachLeftGripperEventHandlers(_leftGripper);
                else DetachLeftGripperEventHandlers(_leftGripper);
            }
        }

        #endregion

        #region Protected Override Methods

        protected override void OnClick()
        {
            base.OnClick();
            OwnerItem?.PerformSort();
        }

        /// <summary>
        /// Обрабатывает правый клик мыши на заголовке колонки.
        /// Для системных заголовков показывает контекстное меню с CheckBox видимости корневых колонок.
        /// Для групп заголовков показывает меню видимости дочерних колонок.
        /// Для листовых колонок показывает меню выбора типа агрегата.
        /// </summary>
        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            e.Handled = true;

            // Для системных заголовков показываем их собственное контекстное меню,
            // установленное через ContextMenu в SystemColumnHeaderItem.SetupContextMenu()
            if (OwnerItem is SystemColumnHeaderItem || OwnerItem is SystemLeafHeaderItem)
            {
                if (ContextMenu != null)
                {
                    ContextMenu.PlacementTarget = this;
                    ContextMenu.IsOpen = true;
                }
                return;
            }

            if (OwnerItem != null && OwnerItem.HasChildren)
            {
                ShowColumnsVisibilityContextMenu();
            }
            else
            {
                ShowAggregateContextMenu();
            }
            base.OnMouseRightButtonDown(e);
        }

        /// <summary>
        /// Показывает контекстное меню с выбором типа агрегата для текущей колонки.
        /// </summary>
        private void ShowColumnsVisibilityContextMenu()
        {
            if (OwnerItem == null) return;

            var contextMenu = new ContextMenu();
            var visibilityMenu = new MenuItem
            {
                Header = LocalizationManager.GetString("ColumnVisibility", "Column Visibility")
            };

            // Добавляем пункт для скрытия самого группового заголовка (если разрешено)
            if (OwnerItem.CanUserHide)
            {
                var hideGroupMenuItem = new MenuItem
                {
                    Header = LocalizationManager.GetString("HideGroup", "Hide Group"),
                    IsChecked = OwnerItem.IsVisible,
                    IsCheckable = true,
                    StaysOpenOnClick = true,
                    Tag = OwnerItem
                };
                hideGroupMenuItem.Click += OnGroupVisibilityMenuItemClick;
                visibilityMenu.Items.Add(hideGroupMenuItem);
            }

            // Разделитель перед списком дочерних колонок
            visibilityMenu.Items.Add(new Separator());

            var leafItems = GetLeafItems(OwnerItem);
            foreach (var leaf in leafItems)
            {
                // Пропускаем колонки, для которых скрытие запрещено
                if (!leaf.CanUserHide)
                    continue;

                var menuItem = new MenuItem
                {
                    Header = leaf.Header ?? LocalizationManager.GetString("NoTitle", "No title"),
                    IsChecked = leaf.IsVisible,
                    IsCheckable = true,
                    StaysOpenOnClick = true,
                    Tag = leaf
                };

                menuItem.Click += OnColumnVisibilityMenuItemClick;
                visibilityMenu.Items.Add(menuItem);
            }

            contextMenu.Items.Add(visibilityMenu);
            ContextMenu = contextMenu;
            contextMenu.PlacementTarget = this;
            contextMenu.IsOpen = true;
        }

        private List<ColumnHeaderItem> GetLeafItems(ColumnHeaderItem item)
        {
            var leaves = new List<ColumnHeaderItem>();
            if (!item.HasChildren)
            {
                leaves.Add(item);
            }
            else
            {
                foreach (var child in item.Children)
                {
                    leaves.AddRange(GetLeafItems(child));
                }
            }
            return leaves;
        }

        private void OnGroupVisibilityMenuItemClick(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Tag is ColumnHeaderItem groupItem)
            {
                bool newVisibility = menuItem.IsChecked == true;

                // Если пытаемся скрыть группу
                if (!newVisibility && groupItem.IsVisible)
                {
                    // Проверяем, можно ли скрыть (останется ли хотя бы одна видимая корневая колонка)
                    var root = groupItem.GetRootItem();
                    var allLeaves = groupItem.GetAllLeafColumns(root);
                    var visibleCount = allLeaves.Count(col => col.IsVisible);

                    if (visibleCount <= 1)
                    {
                        // Отменяем скрытие и блокируем галочку
                        menuItem.IsChecked = true;

                        ShowTooltip(LocalizationManager.GetString("LastColumnError", "Cannot hide the last visible column!"));
                        return;
                    }
                }

                if (groupItem.IsVisible == newVisibility)
                    return;

                groupItem.IsVisible = newVisibility;

                // При показе группы восстанавливаем видимость всех дочерних заголовков
                // (аналогично ToggleHeaderVisibility в SystemColumnHeaderItem)
                if (newVisibility && groupItem.HasChildren)
                {
                    foreach (var child in groupItem.Children)
                    {
                        if (!child.IsVisible)
                            child.IsVisible = true;
                    }
                }
            }
        }

        private void OnColumnVisibilityMenuItemClick(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Tag is ColumnHeaderItem leaf)
            {
                bool newVisibility = menuItem.IsChecked == true;

                // Если пытаемся скрыть колонку
                if (!newVisibility && leaf.IsVisible)
                {
                    // Проверяем, можно ли скрыть
                    var root = leaf.GetRootItem();
                    var allLeaves = leaf.GetAllLeafColumns(root);
                    var visibleCount = allLeaves.Count(col => col.IsVisible);

                    if (visibleCount <= 1)
                    {
                        // Отменяем скрытие и блокируем галочку
                        menuItem.IsChecked = true;

                        // Опционально: показать всплывающую подсказку
                        ShowTooltip(LocalizationManager.GetString("LastColumnError", "Cannot hide the last visible column!"));
                        return;
                    }
                }

                leaf.IsVisible = newVisibility;


            }
        }

        private void ShowTooltip(string message)
        {
            var tooltip = new ToolTip
            {
                Content = message,
                Placement = PlacementMode.Mouse,
                IsOpen = true
            };

            // Закрываем тултип через 2 секунды
            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };
            timer.Tick += (s, args) =>
            {
                tooltip.IsOpen = false;
                timer.Stop();
            };
            timer.Start();
        }

        private void ShowAggregateContextMenu()
        {
            if (OwnerItem == null) return;

            var contextMenu = new ContextMenu();

            // Создаём пункты меню для каждого типа агрегата
            var aggregateTypes = new[]
            {
                new AggregateMenuItemInfo { AggregateType = Headers.AggregateType.None, DisplayName = LocalizationManager.GetString("NoAggregate", "No total") },
                new AggregateMenuItemInfo { AggregateType = Headers.AggregateType.Sum, DisplayName = LocalizationManager.GetString("Sum", "Sum") },
                new AggregateMenuItemInfo { AggregateType = Headers.AggregateType.Average, DisplayName = LocalizationManager.GetString("Average", "Average") },
                new AggregateMenuItemInfo { AggregateType = Headers.AggregateType.Count, DisplayName = LocalizationManager.GetString("Count", "Count") },
                new AggregateMenuItemInfo { AggregateType = Headers.AggregateType.Min, DisplayName = LocalizationManager.GetString("Min", "Minimum") },
                new AggregateMenuItemInfo { AggregateType = Headers.AggregateType.Max, DisplayName = LocalizationManager.GetString("Max", "Maximum") },
            };

            foreach (var info in aggregateTypes)
            {
                var menuItem = new MenuItem
                {
                    Header = info.DisplayName,
                    Tag = info,
                    IsChecked = OwnerItem.AggregateType == info.AggregateType,
                    IsCheckable = true,
                    StaysOpenOnClick = true,
                };

                menuItem.Click += OnAggregateMenuItemClick;
                contextMenu.Items.Add(menuItem);
            }

            // Добавляем разделитель и пункты фильтрации, если фильтрация разрешена
            if (OwnerItem.CanUserFilter)
            {
                contextMenu.Items.Add(new Separator());

                var filterMenuItem = new MenuItem
                {
                    Header = LocalizationManager.GetString("Filter", "Filter"),
                    Tag = OwnerItem,
                    IsCheckable = false,
                };
                filterMenuItem.Click += OnFilterMenuItemClick;
                contextMenu.Items.Add(filterMenuItem);

                // Если фильтр активен, добавляем пункт "Clear Filter"
                if (OwnerItem.Filter != null && OwnerItem.Filter.IsActive)
                {
                    var clearFilterMenuItem = new MenuItem
                    {
                        Header = LocalizationManager.GetString("FilterClear", "Clear Filter"),
                        Tag = OwnerItem,
                        IsCheckable = false,
                    };
                    clearFilterMenuItem.Click += OnClearFilterMenuItemClick;
                    contextMenu.Items.Add(clearFilterMenuItem);
                }
            }

            // Назначаем контекстное меню текущему элементу и открываем его
            ContextMenu = contextMenu;
            contextMenu.PlacementTarget = this;
            contextMenu.IsOpen = true;
        }

        /// <summary>
        /// Обрабатывает клик по пункту меню "Filter".
        /// Открывает Popup с UI фильтра для данной колонки.
        /// </summary>
        private void OnFilterMenuItemClick(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Tag is ColumnHeaderItem headerItem)
            {
                ShowFilterPopup(headerItem);
            }
        }

        /// <summary>
        /// Обрабатывает клик по пункту меню "Clear Filter".
        /// Сбрасывает фильтр для данной колонки.
        /// </summary>
        private void OnClearFilterMenuItemClick(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Tag is ColumnHeaderItem headerItem)
            {
                var grid = ParentGrid;
                if (grid?.FilterService != null)
                {
                    grid.FilterService.ClearFilter(headerItem);
                    UpdateFilterIndicator();
                }
            }
        }

        /// <summary>
        /// Показывает Popup с UI фильтра для указанной колонки.
        /// Тип UI зависит от SortDataType колонки.
        /// </summary>
        private void ShowFilterPopup(ColumnHeaderItem headerItem)
        {
            var grid = ParentGrid;
            if (grid?.FilterService == null) return;

            var filterControl = FilterUIFactory.CreateFilterControl(headerItem, grid.FilterService);

            var popup = new Popup
            {
                PlacementTarget = this,
                Placement = PlacementMode.Bottom,
                StaysOpen = false,
                AllowsTransparency = true,
                Child = filterControl,
                HorizontalOffset = 0,
                VerticalOffset = 2,
            };

            // Подписываемся на изменение фильтра для обновления индикатора
            System.Action onFilterChanged = null;
            onFilterChanged = () =>
            {
                UpdateFilterIndicator();
                grid.FilterService.FilterChanged -= onFilterChanged;
            };
            grid.FilterService.FilterChanged += onFilterChanged;

            popup.IsOpen = true;
        }

        /// <summary>
        /// Обновляет визуальную индикацию фильтра на заголовке колонки.
        /// </summary>
        private void UpdateFilterIndicator()
        {
            if (OwnerItem != null)
            {
                IsFilterActive = OwnerItem.Filter != null && OwnerItem.Filter.IsActive;
            }
        }

        /// <summary>
        /// Обрабатывает выбор пункта меню с типом агрегата.
        /// Снимает отметки со всех остальных пунктов (режим radio-button).
        /// </summary>
        private void OnAggregateMenuItemClick(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem clickedItem && clickedItem.Tag is AggregateMenuItemInfo info)
            {
                // Снимаем отметки со всех пунктов меню
                if (clickedItem.Parent is ContextMenu menu)
                {
                    foreach (var item in menu.Items)
                    {
                        if (item is MenuItem mi && mi != clickedItem)
                        {
                            mi.IsChecked = false;
                        }
                    }
                }

                clickedItem.IsChecked = true;

                if (OwnerItem != null)
                {
                    OwnerItem.AggregateType = info.AggregateType;
                }
            }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == ActualWidthProperty)
            {
                // В отложенном режиме не синхронизируем ширину с DataGridColumn во время перетаскивания
                if (IsDeferredResizeEnabled)
                    return;

                var newValue = (double)e.NewValue;
                if (OwnerItem != null && OwnerItem.SyncColumn != null)
                    OwnerItem.SyncColumn.Width = new DataGridLength(newValue);
            }
        }

        internal void UpdateContent(object header)
        {
            Content = header;
        }

        internal void UpdateHeaderTemplate(DataTemplate template)
        {
            ContentTemplate = template;
        }

        #endregion
    }
}

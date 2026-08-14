using MakroFlexGrid.Filters;
using MakroFlexGrid.Headers;
using MakroFlexGrid.Rows;
using MakroFlexGrid.Sorting;
using MakroFlexGrid.Utilities;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.Generic;
using System.Linq;

namespace MakroFlexGrid.Core
{
    /// <summary>
    /// Аргументы события для клика по ячейке таблицы.
    /// </summary>
    public class CellClickEventArgs : EventArgs
    {
        /// <summary>
        /// Объект данных строки (Item из RowViewModel).
        /// </summary>
        public object Item { get; init; }

        /// <summary>
        /// Заголовок колонки, по которой кликнули.
        /// </summary>
        public ColumnHeaderItem ColumnHeader { get; init; }

        /// <summary>
        /// RowViewModel строки (для обратной связи).
        /// </summary>
        public RowViewModel RowViewModel { get; init; }
    }

    public class CustomDataGrid : DataGrid
    {
        public event EventHandler<object> RowSelected;
        public event EventHandler<object> RowDoubleClicked;

        /// <summary>
        /// Событие возникает при изменении коллекции выбранных элементов
        /// (только в режиме Multiple).
        /// </summary>
        public event EventHandler<IReadOnlyCollection<object>> SelectedItemsChanged;

        /// <summary>
        /// Событие возникает при клике правой кнопкой мыши по ячейке таблицы.
        /// Передаёт информацию о строке (Item) и колонке (ColumnHeader).
        /// </summary>
        public event EventHandler<CellClickEventArgs> CellRightClicked;

        /// <summary>
        /// Вызывает событие CellRightClicked.
        /// </summary>
        public void OnCellRightClicked(RowViewModel vm, ColumnHeaderItem columnHeader)
        {
            if (vm == null || columnHeader == null) return;
            CellRightClicked?.Invoke(this, new CellClickEventArgs
            {
                Item = vm.Item,
                ColumnHeader = columnHeader,
                RowViewModel = vm
            });
        }

        /// <summary>
        /// Пересчитывает итоговые значения в нижней панели.
        /// Вызывается при изменении типа агрегата для какой-либо колонки.
        /// </summary>
        public void RefreshAggregates()
        {
            _bottomPanelPresenter?.UpdateAggregates();
        }

        public void RefreshHeaders()
        {
            _columnHeadersPresenter?.GenerateHeaderElements();
        }

        private void SyncRowSelection()
        {
            if (_unifiedRowsPresenter == null) return;

            if (RowSelectionMode == RowSelectionMode.Multiple)
            {
                _unifiedRowsPresenter.UpdateMultipleSelection(_selectedItems);
            }
            else
            {
                var selectedItem = RowSelectionMode == RowSelectionMode.None ? null : SelectedItem;
                _unifiedRowsPresenter.UpdateRowSelection(selectedItem);
            }
        }

        public static readonly DependencyProperty LeftMarginProperty =
            DependencyProperty.Register(nameof(LeftMargin), typeof(Thickness),
            typeof(CustomDataGrid), new PropertyMetadata(new Thickness(0)));

        public static readonly DependencyProperty RightMarginProperty =
            DependencyProperty.Register(nameof(RightMargin), typeof(Thickness),
            typeof(CustomDataGrid), new PropertyMetadata(new Thickness(0)));

        public static readonly DependencyProperty LeftBottomMarginProperty =
            DependencyProperty.Register(nameof(LeftBottomMargin), typeof(Thickness),
            typeof(CustomDataGrid), new PropertyMetadata(new Thickness(0)));

        public static readonly DependencyProperty RightBottomMarginProperty =
            DependencyProperty.Register(nameof(RightBottomMargin), typeof(Thickness),
            typeof(CustomDataGrid), new PropertyMetadata(new Thickness(0)));

        public static readonly DependencyProperty GridLineBrushProperty =
            DependencyProperty.Register(nameof(GridLineBrush), typeof(Brush),
            typeof(CustomDataGrid), new PropertyMetadata(Brushes.LightGray));

        /// <summary>
        /// Включает/отключает отображение обводки выбранных ячеек.
        /// При false ячейки не получают обводку при выборе строки,
        /// но выделение строки фоном (RowSelectedBackground) продолжает работать.
        /// </summary>
        public static readonly DependencyProperty IsCellSelectionEnabledProperty =
            DependencyProperty.Register(
                nameof(IsCellSelectionEnabled),
                typeof(bool),
                typeof(CustomDataGrid),
                new PropertyMetadata(true, OnCellSelectionPropertyChanged));

        /// <summary>
        /// Кисть для обводки выбранных ячеек.
        /// По умолчанию используется DodgerBlue для контраста с линиями сетки.
        /// </summary>
        public static readonly DependencyProperty CellSelectedBorderBrushProperty =
            DependencyProperty.Register(
                nameof(CellSelectedBorderBrush),
                typeof(Brush),
                typeof(CustomDataGrid),
                new PropertyMetadata(Brushes.DodgerBlue, OnCellSelectionPropertyChanged));

        /// <summary>
        /// Толщина обводки выбранных ячеек со всех сторон.
        /// По умолчанию 1px со всех сторон.
        /// </summary>
        public static readonly DependencyProperty CellSelectedBorderThicknessProperty =
            DependencyProperty.Register(
                nameof(CellSelectedBorderThickness),
                typeof(Thickness),
                typeof(CustomDataGrid),
                new PropertyMetadata(new Thickness(1), OnCellSelectionPropertyChanged));

        private static void OnCellSelectionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = (CustomDataGrid)d;

            // Если выключили выделение ячеек — сбрасываем
            if (e.Property == IsCellSelectionEnabledProperty && !(bool)e.NewValue)
            {
                grid.ClearCellSelection();
            }

            grid._unifiedRowsPresenter?.UpdateRows();
        }

        public static readonly DependencyProperty LeftFrozenPanelBackgroundProperty =
            DependencyProperty.Register(nameof(LeftFrozenPanelBackground), typeof(Brush),
            typeof(CustomDataGrid), new PropertyMetadata(Brushes.Transparent));

        public static readonly DependencyProperty RightFrozenPanelBackgroundProperty =
            DependencyProperty.Register(nameof(RightFrozenPanelBackground), typeof(Brush),
            typeof(CustomDataGrid), new PropertyMetadata(Brushes.Transparent));

        public static readonly DependencyProperty CenterPanelBackgroundProperty =
            DependencyProperty.Register(nameof(CenterPanelBackground), typeof(Brush),
            typeof(CustomDataGrid), new PropertyMetadata(Brushes.Transparent));

        public static readonly DependencyProperty RowBackgroundProperty =
            DependencyProperty.Register(nameof(RowBackground), typeof(Brush),
            typeof(CustomDataGrid), new PropertyMetadata(Brushes.Transparent));

        public static readonly DependencyProperty RowSelectedBackgroundProperty =
            DependencyProperty.Register(nameof(RowSelectedBackground), typeof(Brush),
            typeof(CustomDataGrid), new PropertyMetadata((Brush)new BrushConverter().ConvertFrom("#afedfa")));

        public static readonly DependencyProperty BottomPanelBackgroundProperty =
            DependencyProperty.Register(nameof(BottomPanelBackground), typeof(Brush),
            typeof(CustomDataGrid), new PropertyMetadata(Brushes.Transparent));

        public static readonly DependencyProperty BottomPanelTextProperty =
            DependencyProperty.Register(nameof(BottomPanelText), typeof(string),
            typeof(CustomDataGrid), new PropertyMetadata(string.Empty, OnBottomPanelPropertyChanged));

        public static readonly DependencyProperty PanelTextAlignmentProperty =
            DependencyProperty.Register(nameof(PanelTextAlignment), typeof(HorizontalAlignment),
            typeof(CustomDataGrid), new PropertyMetadata(HorizontalAlignment.Center, OnBottomPanelPropertyChanged));

        public static readonly DependencyProperty BottomPanelTextPositionProperty =
            DependencyProperty.Register(nameof(BottomPanelTextPosition), typeof(BottomPanelViewModel.PanelTextPosition),
            typeof(CustomDataGrid), new PropertyMetadata(BottomPanelViewModel.PanelTextPosition.Top, OnBottomPanelPropertyChanged));

        public static readonly DependencyProperty PanelTextPaddingProperty =
            DependencyProperty.Register(nameof(PanelTextPadding), typeof(Thickness),
            typeof(CustomDataGrid), new PropertyMetadata(new Thickness(0, 2, 0, 2), OnBottomPanelPropertyChanged));

        public static readonly DependencyProperty PanelTextTemplateProperty =
            DependencyProperty.Register(nameof(PanelTextTemplate), typeof(DataTemplate),
            typeof(CustomDataGrid), new PropertyMetadata(null, OnBottomPanelPropertyChanged));

        /// <summary>
        /// Шаблон для дополнительной строки, которая отображается снизу от основной строки таблицы.
        /// </summary>
        public static readonly DependencyProperty BottomRowTemplateProperty =
            DependencyProperty.Register(
                nameof(BottomRowTemplate),
                typeof(DataTemplate),
                typeof(CustomDataGrid),
                new PropertyMetadata(null, OnBottomRowTemplateChanged));

        private static void OnBottomRowTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = (CustomDataGrid)d;
            grid._unifiedRowsPresenter?.UpdateRowBottomTemplate();
        }

        private static void OnBottomPanelPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as CustomDataGrid;
            if (grid == null)
                return;

            grid.SyncBottomPanelProperties();
            grid._bottomPanelPresenter?.UpdateAggregates();
        }

        public static readonly DependencyProperty ShowBottomCellBordersProperty =
            DependencyProperty.Register(nameof(ShowBottomCellBorders), typeof(bool),
            typeof(CustomDataGrid), new PropertyMetadata(true));

        public static readonly DependencyProperty BottomPanelHeightProperty =
            DependencyProperty.Register(nameof(BottomPanelHeight), typeof(double),
            typeof(CustomDataGrid), new PropertyMetadata(20.0));

        public static readonly DependencyProperty ShowScrollBarSpacersProperty =
            DependencyProperty.Register(nameof(ShowScrollBarSpacers), typeof(bool),
            typeof(CustomDataGrid), new PropertyMetadata(false));

        public static readonly DependencyProperty SeparatorWidthProperty =
            DependencyProperty.Register(nameof(SeparatorWidth), typeof(double),
            typeof(CustomDataGrid), new PropertyMetadata(0.0));

        public static readonly DependencyProperty SeparatorBrushProperty =
            DependencyProperty.Register(nameof(SeparatorBrush), typeof(Brush),
            typeof(CustomDataGrid), new PropertyMetadata(Brushes.Gray));

        public bool IsCellSelectionEnabled
        {
            get => (bool)GetValue(IsCellSelectionEnabledProperty);
            set => SetValue(IsCellSelectionEnabledProperty, value);
        }

        public Brush CellSelectedBorderBrush
        {
            get => (Brush)GetValue(CellSelectedBorderBrushProperty);
            set => SetValue(CellSelectedBorderBrushProperty, value);
        }

        public Thickness CellSelectedBorderThickness
        {
            get => (Thickness)GetValue(CellSelectedBorderThicknessProperty);
            set => SetValue(CellSelectedBorderThicknessProperty, value);
        }

        public int LeftFrozenColumnsCount => FrozenColumnHeaders.GetBottomItems().Length;

        public int RightFrozenColumnsCount => RightFrozenColumnHeaders.GetBottomItems().Length;

        public Thickness LeftMargin
        {
            get => (Thickness)GetValue(LeftMarginProperty);
            set => SetValue(LeftMarginProperty, value);
        }

        public Thickness RightMargin
        {
            get => (Thickness)GetValue(RightMarginProperty);
            set => SetValue(RightMarginProperty, value);
        }

        public Thickness LeftBottomMargin
        {
            get => (Thickness)GetValue(LeftBottomMarginProperty);
            set => SetValue(LeftBottomMarginProperty, value);
        }

        public Thickness RightBottomMargin
        {
            get => (Thickness)GetValue(RightBottomMarginProperty);
            set => SetValue(RightBottomMarginProperty, value);
        }

        public Brush GridLineBrush
        {
            get => (Brush)GetValue(GridLineBrushProperty);
            set => SetValue(GridLineBrushProperty, value);
        }

        public Brush LeftFrozenPanelBackground
        {
            get => (Brush)GetValue(LeftFrozenPanelBackgroundProperty);
            set => SetValue(LeftFrozenPanelBackgroundProperty, value);
        }

        public Brush RightFrozenPanelBackground
        {
            get => (Brush)GetValue(RightFrozenPanelBackgroundProperty);
            set => SetValue(RightFrozenPanelBackgroundProperty, value);
        }

        public Brush CenterPanelBackground
        {
            get => (Brush)GetValue(CenterPanelBackgroundProperty);
            set => SetValue(CenterPanelBackgroundProperty, value);
        }

        public Brush RowBackground
        {
            get => (Brush)GetValue(RowBackgroundProperty);
            set => SetValue(RowBackgroundProperty, value);
        }

        public Brush RowSelectedBackground
        {
            get => (Brush)GetValue(RowSelectedBackgroundProperty);
            set => SetValue(RowSelectedBackgroundProperty, value);
        }

        public Brush BottomPanelBackground
        {
            get => (Brush)GetValue(BottomPanelBackgroundProperty);
            set => SetValue(BottomPanelBackgroundProperty, value);
        }

        public string BottomPanelText
        {
            get => (string)GetValue(BottomPanelTextProperty);
            set => SetValue(BottomPanelTextProperty, value);
        }

        public HorizontalAlignment PanelTextAlignment
        {
            get => (HorizontalAlignment)GetValue(PanelTextAlignmentProperty);
            set => SetValue(PanelTextAlignmentProperty, value);
        }

        public BottomPanelViewModel.PanelTextPosition BottomPanelTextPosition
        {
            get => (BottomPanelViewModel.PanelTextPosition)GetValue(BottomPanelTextPositionProperty);
            set => SetValue(BottomPanelTextPositionProperty, value);
        }

        public Thickness PanelTextPadding
        {
            get => (Thickness)GetValue(PanelTextPaddingProperty);
            set => SetValue(PanelTextPaddingProperty, value);
        }

        public DataTemplate PanelTextTemplate
        {
            get => (DataTemplate)GetValue(PanelTextTemplateProperty);
            set => SetValue(PanelTextTemplateProperty, value);
        }

        /// <summary>
        /// DataTemplate for additional row below main row.
        /// Similar to RowDetailsTemplate but displayed at bottom.
        /// If not set, additional row is not displayed.
        /// DataContext is the row data item.
        /// </summary>
        public DataTemplate BottomRowTemplate
        {
            get => (DataTemplate)GetValue(BottomRowTemplateProperty);
            set => SetValue(BottomRowTemplateProperty, value);
        }

        /// <summary>
        /// Показывать ли бордюры ячеек в нижней панели итогов.
        /// По умолчанию true (бордюры отображаются).
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

        public bool ShowScrollBarSpacers
        {
            get => (bool)GetValue(ShowScrollBarSpacersProperty);
            set => SetValue(ShowScrollBarSpacersProperty, value);
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

        /// <summary>
        /// Включает/отключает отображение системной колонки (с треугольником выделения)
        /// в левой части строк. По умолчанию true (системная колонка отображается).
        /// </summary>
        public static readonly DependencyProperty IsSystemColumnEnabledProperty =
            DependencyProperty.Register(
                nameof(IsSystemColumnEnabled),
                typeof(bool),
                typeof(CustomDataGrid),
                new PropertyMetadata(true, OnIsSystemColumnEnabledChanged));

        private static void OnIsSystemColumnEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = (CustomDataGrid)d;
            grid._unifiedRowsPresenter?.UpdateRows();
            grid._columnHeadersPresenter?.GenerateHeaderElements();
            grid._bottomPanelPresenter?.UpdateAggregates();
        }

        public bool IsSystemColumnEnabled
        {
            get => (bool)GetValue(IsSystemColumnEnabledProperty);
            set => SetValue(IsSystemColumnEnabledProperty, value);
        }

        public static readonly DependencyProperty IsDeferredResizeEnabledProperty =
            DependencyProperty.Register(
                nameof(IsDeferredResizeEnabled),
                typeof(bool),
                typeof(CustomDataGrid),
                new PropertyMetadata(false));

        public bool IsDeferredResizeEnabled
        {
            get => (bool)GetValue(IsDeferredResizeEnabledProperty);
            set => SetValue(IsDeferredResizeEnabledProperty, value);
        }

        /// <summary>
        /// Глобально включает/отключает возможность перетаскивания (Drag & Drop) заголовков колонок.
        /// По умолчанию true (DnD разрешён).
        /// </summary>
        public static readonly DependencyProperty AllowDragProperty =
            DependencyProperty.Register(
                nameof(AllowDrag),
                typeof(bool),
                typeof(CustomDataGrid),
                new PropertyMetadata(true));

        public bool AllowDrag
        {
            get => (bool)GetValue(AllowDragProperty);
            set => SetValue(AllowDragProperty, value);
        }

        /// <summary>
        /// DependencyProperty для режима выбора строк.
        /// </summary>
        public static readonly DependencyProperty RowSelectionModeProperty =
            DependencyProperty.Register(
                nameof(RowSelectionMode),
                typeof(RowSelectionMode),
                typeof(CustomDataGrid),
                new PropertyMetadata(RowSelectionMode.Single, OnRowSelectionModeChanged));

        private static void OnRowSelectionModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = (CustomDataGrid)d;
            var newMode = (RowSelectionMode)e.NewValue;

            if (newMode == RowSelectionMode.None)
            {
                // Сбрасываем все выделения
                grid._selectedItems.Clear();
                grid._lastSelectedItem = null;
                grid.SetCurrentValue(SelectedItemProperty, null);
                grid.SyncRowSelection();
            }
            else if (newMode == RowSelectionMode.Single)
            {
                // Сбрасываем множественное выделение, оставляем только SelectedItem
                grid._selectedItems.Clear();
                if (grid.SelectedItem != null)
                {
                    grid._selectedItems.Add(grid.SelectedItem);
                }
                grid.SyncRowSelection();
            }
            // При переходе в Multiple — существующие выделения сохраняются
        }

        public RowSelectionMode RowSelectionMode
        {
            get => (RowSelectionMode)GetValue(RowSelectionModeProperty);
            set => SetValue(RowSelectionModeProperty, value);
        }

        // Хранилище выбранных элементов для Multiple режима
        private readonly HashSet<object> _selectedItems = new HashSet<object>();

        // Последняя выбранная строка (для Shift+Click)
        private object _lastSelectedItem;

        /// <summary>
        /// Коллекция выбранных элементов (работает во всех режимах).
        /// В режиме Single содержит 0 или 1 элемент.
        /// В режиме None всегда пуста.
        /// </summary>
        public IReadOnlyCollection<object> SelectedItems => _selectedItems.ToList().AsReadOnly();

        /// <summary>
        /// Сервис фильтрации колонок. Управляет фильтрами и применяет их к данным.
        /// </summary>
        public FilterService FilterService { get; }

        #region Column Headers (многоуровневые заголовки)

        /// <summary>
        /// DependencyProperty для FrozenColumnHeaders.
        /// Позволяет задавать заголовки из XAML.
        /// </summary>
        public static readonly DependencyProperty FrozenColumnHeadersProperty =
            DependencyProperty.Register(
                nameof(FrozenColumnHeaders),
                typeof(ColumnHeaderCollection),
                typeof(CustomDataGrid),
                new PropertyMetadata(null, OnColumnHeadersCollectionChanged));

        /// <summary>
        /// DependencyProperty для ScrollableColumnHeaders.
        /// Позволяет задавать заголовки из XAML.
        /// </summary>
        public static readonly DependencyProperty ScrollableColumnHeadersProperty =
            DependencyProperty.Register(
                nameof(ScrollableColumnHeaders),
                typeof(ColumnHeaderCollection),
                typeof(CustomDataGrid),
                new PropertyMetadata(null, OnColumnHeadersCollectionChanged));

        /// <summary>
        /// DependencyProperty для RightFrozenColumnHeaders.
        /// Позволяет задавать заголовки из XAML.
        /// </summary>
        public static readonly DependencyProperty RightFrozenColumnHeadersProperty =
            DependencyProperty.Register(
                nameof(RightFrozenColumnHeaders),
                typeof(ColumnHeaderCollection),
                typeof(CustomDataGrid),
                new PropertyMetadata(null, OnColumnHeadersCollectionChanged));

        /// <summary>
        /// Callback при изменении коллекции заголовков.
        /// Устанавливает OwnerGrid для новой коллекции.
        /// </summary>
        private static void OnColumnHeadersCollectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = (CustomDataGrid)d;
            if (e.NewValue is ColumnHeaderCollection collection)
            {
                collection.OwnerGrid = grid;
            }
        }

        /// <summary>
        /// Замороженные (левые) заголовки колонок.
        /// </summary>
        public ColumnHeaderCollection FrozenColumnHeaders
        {
            get => (ColumnHeaderCollection)GetValue(FrozenColumnHeadersProperty);
            set => SetValue(FrozenColumnHeadersProperty, value);
        }

        /// <summary>
        /// Скроллируемые (центральные) заголовки колонок.
        /// </summary>
        public ColumnHeaderCollection ScrollableColumnHeaders
        {
            get => (ColumnHeaderCollection)GetValue(ScrollableColumnHeadersProperty);
            set => SetValue(ScrollableColumnHeadersProperty, value);
        }

        /// <summary>
        /// Замороженные (правые) заголовки колонок.
        /// </summary>
        public ColumnHeaderCollection RightFrozenColumnHeaders
        {
            get => (ColumnHeaderCollection)GetValue(RightFrozenColumnHeadersProperty);
            set => SetValue(RightFrozenColumnHeadersProperty, value);
        }

        #endregion

        /// <summary>
        /// Словарь для хранения связи между DataGridColumn и ColumnHeaderItem.
        /// Используется вместо отсутствующего свойства Tag у DataGridColumn.
        /// </summary>
        private readonly Dictionary<DataGridColumn, ColumnHeaderItem> _columnToHeaderMap = new Dictionary<DataGridColumn, ColumnHeaderItem>();

        private UnifiedRowsPresenter _unifiedRowsPresenter;

        // Поля для хранения делегатов подписок, чтобы можно было отписаться при повторном OnApplyTemplate
        private RoutedPropertyChangedEventHandler<double> _horizontalScrollBarHandler;
        private EventHandler<double> _horizontalScrollChangedHandler;
        private DependencyPropertyDescriptor _itemsSourceDescriptor;
        private System.Windows.Threading.DispatcherOperation _frozenBorderUpdateOperation;

        // Guard для предотвращения повторного входа в SyncColumnsWithHeaders
        private bool _isSyncingColumns;
        // Флаг для debounce в OnHeaderCollectionChanged
        private bool _pendingSync;
        // Флаг, указывающий, что SyncColumnsWithHeaders() уже была выполнена синхронно
        // (из ExecuteDrop), и отложенный вызов из OnHeaderCollectionChanged нужно пропустить.
        private bool _syncAlreadyExecuted;

        // Текущая выбранная ячейка (для режима выделения ячеек)
        private CellViewModel _selectedCell;

        /// <summary>
        /// Сбрасывает выделение текущей ячейки.
        /// </summary>
        private void ClearCellSelection()
        {
            if (_selectedCell != null)
            {
                _selectedCell.IsCellSelected = false;
                _selectedCell = null;
            }
        }

        /// <summary>
        /// Выделяет строку (без сброса выделения ячейки).
        /// Используется из OnRowClicked (со сбросом ячейки) и OnCellClicked (без сброса ячейки).
        /// </summary>
        private void SelectRow(RowViewModel vm)
        {
            if (vm == null) return;

            if (RowSelectionMode == RowSelectionMode.None)
                return;

            if (RowSelectionMode == RowSelectionMode.Multiple)
            {
                HandleMultipleSelectionClick(vm);
                return;
            }

            // Single mode (original behavior)
            // SetCurrentValue корректно обновляет DependencyProperty SelectedItem,
            // что вызывает SelectionChanged routed event и все внутренние механизмы DataGrid.
            // В отличие от прямой установки base.SelectedItem, SetCurrentValue сохраняет
            // возможные binding и не помечает свойство как локально установленное.
            SetCurrentValue(SelectedItemProperty, vm.Item);

            RowSelected?.Invoke(this, vm.Item);
            SyncRowSelection();
        }

        public void OnRowClicked(RowViewModel vm)
        {
            if (vm == null) return;

            // При клике на строку (не на ячейку) сбрасываем выделение ячейки
            ClearCellSelection();

            SelectRow(vm);
        }

        /// <summary>
        /// Обрабатывает клик в режиме Multiple с учётом модификаторов Ctrl/Shift.
        /// </summary>
        private void HandleMultipleSelectionClick(RowViewModel vm)
        {
            bool isCtrlPressed = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            bool isShiftPressed = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);

            if (isShiftPressed && _lastSelectedItem != null)
            {
                // Shift+Click: выбираем диапазон от _lastSelectedItem до vm.Item
                SelectRange(_lastSelectedItem, vm.Item);
            }
            else if (isCtrlPressed)
            {
                // Ctrl+Click: toggle выбора текущей строки
                if (_selectedItems.Contains(vm.Item))
                    _selectedItems.Remove(vm.Item);
                else
                    _selectedItems.Add(vm.Item);

                _lastSelectedItem = vm.Item;
            }
            else
            {
                // Обычный клик: сбрасываем все и выбираем только текущую
                _selectedItems.Clear();
                _selectedItems.Add(vm.Item);
                _lastSelectedItem = vm.Item;
            }

            // Всегда устанавливаем SelectedItem на последнюю кликнутую строку
            SetCurrentValue(SelectedItemProperty, vm.Item);

            RowSelected?.Invoke(this, vm.Item);
            SyncRowSelection();
            SelectedItemsChanged?.Invoke(this, _selectedItems.ToList().AsReadOnly());
        }

        /// <summary>
        /// Выбирает диапазон строк от fromItem до toItem включительно.
        /// Используется при Shift+Click.
        /// </summary>
        private void SelectRange(object fromItem, object toItem)
        {
            var itemsControl = _unifiedRowsPresenter?.ItemsControl;
            if (itemsControl == null)
                return;

            int fromIndex = -1;
            int toIndex = -1;

            for (int i = 0; i < itemsControl.Items.Count; i++)
            {
                var item = itemsControl.Items[i];
                if (ReferenceEquals(item, fromItem))
                    fromIndex = i;
                if (ReferenceEquals(item, toItem))
                    toIndex = i;
            }

            if (fromIndex == -1 || toIndex == -1)
                return;

            int start = Math.Min(fromIndex, toIndex);
            int end = Math.Max(fromIndex, toIndex);

            for (int i = start; i <= end; i++)
            {
                var item = itemsControl.Items[i];
                _selectedItems.Add(item);
            }
        }

        /// <summary>
        /// Вызывается при клике левой кнопкой по ячейке.
        /// Если IsCellSelectionEnabled включён, устанавливает выбранную ячейку.
        /// </summary>
        public void OnCellClicked(CellViewModel cellVm)
        {
            if (cellVm == null) return;
            if (!IsCellSelectionEnabled) return;

            // Снимаем выделение с предыдущей ячейки
            if (_selectedCell != null)
            {
                _selectedCell.IsCellSelected = false;
            }

            // Выделяем новую ячейку
            _selectedCell = cellVm;
            _selectedCell.IsCellSelected = true;

            // Также выделяем строку, чтобы SelectedItem обновился
            var rowVm = cellVm.RowViewModel;
            if (rowVm != null)
            {
                SelectRow(rowVm);
            }
        }

        public void OnRowDoubleClicked(RowViewModel vm)
        {
            if (vm == null) return;
            RowDoubleClicked?.Invoke(this, vm.Item);
        }
        private ScrollBar _horizontalScrollBar;
        private Border _leftScrollBarSpacer;
        private Border _rightScrollBarSpacer;
        private ColumnHeadersPresenter _columnHeadersPresenter;
        private Border _leftFrozenBorder;
        private Border _rightFrozenBorder;
        private BottomPanelPresenter _bottomPanelPresenter;

        public BottomPanelViewModel BottomPanel
        {
            get => _bottomPanelPresenter?.ViewModel;
            set
            {
                if (_bottomPanelPresenter != null && _bottomPanelPresenter.ViewModel != value)
                {
                    _bottomPanelPresenter.ViewModel = value;
                    SyncBottomPanelProperties();
                }
            }
        }

        private void SyncBottomPanelProperties()
        {
            var vm = BottomPanel;
            if (vm == null) return;

            vm.PanelTextAlignment = PanelTextAlignment;
            vm.TextPosition = BottomPanelTextPosition;
            vm.PanelTextPadding = PanelTextPadding;
            vm.PanelTextTemplate = PanelTextTemplate;
        }
        private bool _isUpdatingScrollBar;

        static CustomDataGrid()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomDataGrid),
                new FrameworkPropertyMetadata(typeof(CustomDataGrid)));
        }

        public CustomDataGrid()
        {
            FilterService = new FilterService(this);

            System.Diagnostics.Debug.WriteLine("[CustomDataGrid] Constructor called");

            // Подписываемся на SelectionChanged для синхронизации IsSelected в RowViewModel
            // при любом изменении выбора (не только через клик, но и через binding, код и т.д.)
            SelectionChanged += OnSelectionChanged;

            EnableColumnVirtualization = true;
            EnableRowVirtualization = true;

            VirtualizingPanel.SetIsVirtualizing(this, true);
            VirtualizingPanel.SetVirtualizationMode(this, VirtualizationMode.Recycling);
            VirtualizingPanel.SetScrollUnit(this, ScrollUnit.Pixel);

            VirtualizingPanel.SetCacheLength(this, new VirtualizationCacheLength(2));
            VirtualizingPanel.SetCacheLengthUnit(this, VirtualizationCacheLengthUnit.Page);

            // Инициализируем коллекции заголовков через SetValue,
            // чтобы корректно работали DependencyProperty и XAML-парсер.
            // Если коллекция уже была установлена из XAML (до вызова конструктора),
            // пропускаем инициализацию.
            if (FrozenColumnHeaders == null)
                SetValue(FrozenColumnHeadersProperty, new ColumnHeaderCollection(this));
            if (ScrollableColumnHeaders == null)
                SetValue(ScrollableColumnHeadersProperty, new ColumnHeaderCollection(this));
            if (RightFrozenColumnHeaders == null)
                SetValue(RightFrozenColumnHeadersProperty, new ColumnHeaderCollection(this));

            AttachHeaderEventHandlers();

            // Подписываемся на Loaded, чтобы проверить, применяется ли шаблон
            Loaded += OnCustomDataGridLoaded;
        }

        private void OnCustomDataGridLoaded(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"[CustomDataGrid] Loaded event, Template={Template != null}, _unifiedRowsPresenter={_unifiedRowsPresenter != null}");
        }

        #region Header Event Handlers

        private void AttachHeaderEventHandlers()
        {
            FrozenColumnHeaders.CollectionChanged += OnHeaderCollectionChanged;
            ScrollableColumnHeaders.CollectionChanged += OnHeaderCollectionChanged;
            RightFrozenColumnHeaders.CollectionChanged += OnHeaderCollectionChanged;
        }

        private void OnHeaderCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Debounce: при множественных изменениях коллекции (Remove+Add+Move при Drag&Drop)
            // синхронизация выполняется только один раз через Dispatcher.
            // Это предотвращает многократные полные перестроения UI.
            if (_pendingSync) return;
            _pendingSync = true;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                _pendingSync = false;
                // Если SyncColumnsWithHeaders() уже была выполнена синхронно (из ExecuteDrop),
                // пропускаем отложенный вызов, чтобы избежать двойной синхронизации.
                if (!_syncAlreadyExecuted)
                {
                    SyncColumnsWithHeaders();
                }
                _syncAlreadyExecuted = false;
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        #endregion

        #region Column Headers Presenter

        internal ColumnHeadersPresenter ColumnHeadersPresenter
        {
            get => _columnHeadersPresenter;
            private set => _columnHeadersPresenter = value;
        }

        #endregion

        #region Sync Columns With Headers

        /// <summary>
        /// Синхронизирует Columns DataGrid с иерархией ColumnHeaderItem.
        /// Создаёт DataGridTextColumn для каждого листового элемента заголовка.
        /// </summary>
        public void SyncColumnsWithHeaders()
        {
            // Guard: предотвращает повторный вход при каскадных изменениях коллекций.
            // Без этого guard при Drag&Drop может быть несколько вложенных вызовов,
            // каждый из которых перестраивает заголовки и пересоздаёт строки.
            if (_isSyncingColumns) return;

            // Guard: пропускаем синхронизацию, если шаблон ещё не применён
            // (OnApplyTemplate не вызывался) или элемент выгружен.
            // _columnHeadersPresenter устанавливается в OnApplyTemplate().
            if (_columnHeadersPresenter == null)
                return;

            _isSyncingColumns = true;
            try
            {
                _columnHeadersPresenter?.GenerateHeaderElements();

                // Получаем все листовые колонки, независимо от видимости
                var frozenBottomItems = FrozenColumnHeaders.GetBottomItems().ToArray();
                var scrollableBottomItems = ScrollableColumnHeaders.GetBottomItems().ToArray();
                var rightFrozenBottomItems = RightFrozenColumnHeaders.GetBottomItems().ToArray();

                // ВАЖНО: Синхронизируем DataGrid.Columns ДО вызова UpdateRows(),
                // чтобы PrepareContainerForItemOverride получил уже синхронизированные колонки.
                // Иначе RowViewModel.UpdateCells() будет использовать неактуальный порядок колонок,
                // что приведёт к схлопыванию правой панели после Drag&Drop.
                var totalBottomItems = new List<ColumnHeaderItem>();
                totalBottomItems.AddRange(frozenBottomItems);
                totalBottomItems.AddRange(scrollableBottomItems);
                totalBottomItems.AddRange(rightFrozenBottomItems);

                var totalBottomItemsHash = new HashSet<ColumnHeaderItem>(totalBottomItems);

                // Удаляем колонки, которые больше не нужны или стали скрытыми
                foreach (var column in Columns.ToArray())
                {
                    if (!(column is DataGridTextColumn) ||
                        !totalBottomItemsHash.Contains(GetColumnHeaderItem(column)))
                    {
                        _columnToHeaderMap.Remove(column);
                        Columns.Remove(column);
                    }
                }

                // Вставляем колонки
                for (int i = 0; i < totalBottomItems.Count; i++)
                {
                    var headerItem = totalBottomItems[i];

                    // Создаём или находим существующую колонку
                    DataGridColumn column = null;
                    foreach (var col in Columns)
                    {
                        if (GetColumnHeaderItem(col) == headerItem)
                        {
                            column = col;
                            break;
                        }
                    }

                    if (column == null)
                    {
                        column = CreateColumnForHeader(headerItem);
                        headerItem.SyncColumn = column;
                    }

                    // Синхронизируем видимость колонки с заголовком
                    column.Visibility = headerItem.IsVisible ? Visibility.Visible : Visibility.Collapsed;

                    // Используем IndexOf для получения реальной позиции колонки в коллекции Columns,
                    // а не DisplayIndex. DisplayIndex — это отдельное свойство WPF DataGrid,
                    // которое может не соответствовать индексу в коллекции Columns.
                    // Использование DisplayIndex в Columns.Move() приводит к рассинхронизации
                    // заголовков с ячейками после Drag&Drop.
                    var currentIndex = Columns.IndexOf(column);
                    if (currentIndex != i)
                    {
                        if (currentIndex == -1)
                            Columns.Insert(i, column);
                        else
                            Columns.Move(currentIndex, i);
                    }

                    if (headerItem.SortDirection != null)
                        PerformSort(column);
                }

                // Синхронизируем счетчики в презентере строк (теперь DataGrid.Columns уже синхронизированы).
                // OnPropertyChangedThrottled теперь вызывает только UpdateMaxHorizontalOffset(),
                // но не UpdateRows(). UpdateRows() вызываем один раз вручную после установки обоих счетчиков.
                if (_unifiedRowsPresenter != null)
                {
                    _unifiedRowsPresenter.LeftFrozenColumnsCount = frozenBottomItems.Count();
                    _unifiedRowsPresenter.RightFrozenColumnsCount = rightFrozenBottomItems.Count();
                    _unifiedRowsPresenter.UpdateRows();
                }

                // Пересчитываем агрегаты в нижней панели после синхронизации колонок
                _bottomPanelPresenter?.UpdateAggregates();
            }
            finally
            {
                _isSyncingColumns = false;
            }
        }

        /// <summary>
        /// Уведомляет CustomDataGrid, что SyncColumnsWithHeaders() была выполнена синхронно
        /// (например, из ExecuteDrop), чтобы отложенный вызов из OnHeaderCollectionChanged
        /// был пропущен.
        /// </summary>
        internal void NotifySyncExecuted()
        {
            _syncAlreadyExecuted = true;
        }

        /// <summary>
        /// Обновляет строки при изменении видимости колонок.
        /// Сохраняет и восстанавливает позицию скролла.
        /// </summary>
        internal void RefreshRows()
        {
            if (_unifiedRowsPresenter == null) return;

            // Вместо полного пересоздания ItemsSource (что убивает виртуализацию)
            // обновляем только CellViewModel в существующих строках.
            // Это на порядки быстрее при скрытии/показе колонок или Drag&Drop.
            _unifiedRowsPresenter.RefreshRowCells();

            // Обновляем агрегаты в нижней панели после обновления строк
            RefreshAggregates();
        }





        internal ColumnHeaderItem GetColumnHeaderItem(DataGridColumn column)
        {
            if (column == null) return null;
            _columnToHeaderMap.TryGetValue(column, out var headerItem);
            return headerItem;
        }

        private DataGridColumn CreateColumnForHeader(ColumnHeaderItem headerItem)
        {
            var column = new DataGridTextColumn
            {
                Header = headerItem.Header,
                Width = new DataGridLength(headerItem.Width),
                MinWidth = headerItem.MinWidth,
                MaxWidth = headerItem.MaxWidth,
                CanUserSort = headerItem.CanUserSort,
                SortDirection = headerItem.SortDirection,
                SortMemberPath = headerItem.SortMemberPath,
            };

            // Сохраняем связь в словаре
            _columnToHeaderMap[column] = headerItem;

            // Создаём привязку к свойству, указанному в SortMemberPath
            if (!string.IsNullOrEmpty(headerItem.SortMemberPath))
            {
                column.Binding = new Binding(headerItem.SortMemberPath);
            }

            return column;
        }

        #endregion

        #region Sort

        /// <summary>
        /// Запускает сортировку по указанной колонке.
        /// </summary>
        public void PerformSort(DataGridColumn sortColumn)
        {
            if (!CanUserSortColumns)
                return;

            if (CommitEdit())
            {
                PrepareForSort(sortColumn);

                var args = new DataGridSortingEventArgs(sortColumn);
                OnSorting(args);

                if (Items.NeedsRefresh)
                {
                    try
                    {
                        Items.Refresh();
                    }
                    catch
                    {
                        Items.SortDescriptions.Clear();
                    }
                }

                // Принудительная синхронизация скролла после сортировки,
                // т.к. Items.Refresh() может сбросить HorizontalOffset в ScrollViewer
                ForceSyncScrollAfterSort();
            }
        }

        /// <summary>
        /// Принудительно синхронизирует горизонтальный скролл заголовков и строк
        /// после сортировки, когда Items.Refresh() может сбросить HorizontalOffset
        /// во внутреннем ScrollViewer.
        /// </summary>
        private void ForceSyncScrollAfterSort()
        {
            if (_unifiedRowsPresenter == null) return;

            var scrollManager = _unifiedRowsPresenter.ScrollManager;
            if (scrollManager == null) return;

            double offset = scrollManager.HorizontalOffset;

            // Синхронизируем ScrollViewer в строках
            _unifiedRowsPresenter.ForceSyncScroll(offset);

            // Синхронизируем заголовки
            _columnHeadersPresenter?.SyncScrollOffset(offset);
        }

        private void PrepareForSort(DataGridColumn sortColumn)
        {
            if (Keyboard.IsKeyDown(Key.LeftShift)
                || !Columns.Contains(sortColumn))
                return;

            if (Columns != null)
            {
                foreach (DataGridColumn column in Columns)
                {
                    if (column != sortColumn)
                    {
                        column.SortDirection = null;

                        // Синхронизируем SortDirection обратно в ColumnHeaderItem,
                        // чтобы стрелка сортировки скрылась у неактивных колонок
                        var headerItem = GetColumnHeaderItem(column);
                        if (headerItem != null)
                            headerItem.SortDirection = null;
                    }
                }
            }
        }

        protected override void OnSorting(DataGridSortingEventArgs eventArgs)
        {
            var column = eventArgs.Column;
            var headerItem = GetColumnHeaderItem(column);

            if (headerItem != null && headerItem.CanUserSort)
            {
                ListSortDirection direction = column.SortDirection != null
                    ? (column.SortDirection.Value == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending)
                    : ListSortDirection.Ascending;

                if (headerItem.SortDataType == SortDataType.Text)
                {
                    base.OnSorting(eventArgs);

                    // После базовой сортировки синхронизируем SortDirection обратно в ColumnHeaderItem
                    var textHeaderItem = GetColumnHeaderItem(column);
                    if (textHeaderItem != null)
                        textHeaderItem.SortDirection = column.SortDirection;

                    return;
                }

                eventArgs.Handled = true;

                var comparer = SortComparerFactory.GetComparer(headerItem.SortDataType, direction);

                var collectionView = CollectionViewSource.GetDefaultView(ItemsSource) as ListCollectionView;
                if (collectionView != null)
                {
                    collectionView.CustomSort = comparer;
                    collectionView.SortDescriptions.Clear();
                    collectionView.SortDescriptions.Add(new SortDescription(headerItem.SortMemberPath, direction));
                    collectionView.Refresh();
                }

                column.SortDirection = direction;

                // Синхронизируем SortDirection обратно в ColumnHeaderItem,
                // чтобы стрелка сортировки отобразилась в заголовке
                var currentHeaderItem = GetColumnHeaderItem(column);
                if (currentHeaderItem != null)
                    currentHeaderItem.SortDirection = direction;

                return;
            }

            base.OnSorting(eventArgs);
        }

        #endregion

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _unifiedRowsPresenter = GetTemplateChild("PART_CentralRowsPresenter") as UnifiedRowsPresenter;
            _horizontalScrollBar = GetTemplateChild("PART_HorizontalScrollBar") as ScrollBar;
            _columnHeadersPresenter = GetTemplateChild("PART_ColumnHeadersPresenter") as ColumnHeadersPresenter;
            _leftScrollBarSpacer = GetTemplateChild("PART_LeftScrollBarSpacer") as Border;
            _rightScrollBarSpacer = GetTemplateChild("PART_RightScrollBarSpacer") as Border;
            _bottomPanelPresenter = GetTemplateChild("PART_BottomPanelPresenter") as BottomPanelPresenter;

            System.Diagnostics.Debug.WriteLine($"[CustomDataGrid] OnApplyTemplate: _unifiedRowsPresenter={_unifiedRowsPresenter != null}, ItemsSource={ItemsSource?.GetType().Name ?? "null"}");

            if (_horizontalScrollBar != null && _columnHeadersPresenter != null)
            {
                // Очищаем старый Binding перед созданием нового (защита от повторного OnApplyTemplate)
                BindingOperations.ClearBinding(_horizontalScrollBar, RangeBase.MaximumProperty);
#if DEBUG
                MemoryDiagnostics.OnBindingCleared();
#endif

                var maxBinding = new Binding
                {
                    Source = _columnHeadersPresenter,
                    Path = new PropertyPath("ScrollableWidth"),
                    Mode = BindingMode.OneWay
                };
                _horizontalScrollBar.SetBinding(RangeBase.MaximumProperty, maxBinding);

#if DEBUG
                MemoryDiagnostics.OnBindingCreated();
#endif

                UpdateScrollBarViewportSize();

                // Отписываем предыдущий обработчик, если он был (защита от повторного OnApplyTemplate)
                if (_horizontalScrollBarHandler != null)
                    _horizontalScrollBar.ValueChanged -= _horizontalScrollBarHandler;

                // TwoWay: ScrollBar > ScrollManager (через него обновляются заголовки и ScrollViewer)
                _horizontalScrollBarHandler = (s, e) =>
                {
                    if (_isUpdatingScrollBar) return;

                    var scrollManager = _unifiedRowsPresenter.ScrollManager;
                    if (scrollManager != null && Math.Abs(scrollManager.HorizontalOffset - e.NewValue) > 0.01)
                    {
                        scrollManager.HorizontalOffset = e.NewValue;
                    }
                };
                _horizontalScrollBar.ValueChanged += _horizontalScrollBarHandler;

                // Отписываем предыдущий обработчик, если он был (защита от повторного OnApplyTemplate)
                if (_horizontalScrollChangedHandler != null)
                    _unifiedRowsPresenter.HorizontalScrollChanged -= _horizontalScrollChangedHandler;

                // TwoWay: ScrollViewer > ScrollBar
                _horizontalScrollChangedHandler = (s, offset) =>
                {
                    if (_isUpdatingScrollBar) return;

                    _isUpdatingScrollBar = true;
                    try
                    {
                        if (_horizontalScrollBar != null && Math.Abs(_horizontalScrollBar.Value - offset) > 0.01)
                        {
                            _horizontalScrollBar.Value = offset;
                        }

                        // Синхронизируем заголовки
                        _columnHeadersPresenter?.SyncScrollOffset(offset);
                    }
                    finally
                    {
                        _isUpdatingScrollBar = false;
                    }
                };
                _unifiedRowsPresenter.HorizontalScrollChanged += _horizontalScrollChangedHandler;

                _horizontalScrollBar.Minimum = 0;
            }

            if (_unifiedRowsPresenter != null)
            {
                _unifiedRowsPresenter.SetParentGrid(this);

                // Синхронизируем ItemsSource вручную, так как биндинг через
                // RelativeSource TemplatedParent может не сработать для ItemsSource,
                // унаследованного от ItemsControl через несколько уровней наследования.
                System.Diagnostics.Debug.WriteLine($"[CustomDataGrid] Setting UnifiedRowsPresenter.ItemsSource = {ItemsSource?.GetType().Name ?? "null"}");
                _unifiedRowsPresenter.ItemsSource = ItemsSource;

                // Подписываемся на изменение ItemsSource в CustomDataGrid.
                // Используем DependencyPropertyDescriptor, так как CustomDataGrid (наследник DataGrid)
                // не реализует INotifyPropertyChanged, и PropertyChangedEventManager не может быть использован.
                // Отписка выполняется при повторном OnApplyTemplate через сохранённый дескриптор.
                _itemsSourceDescriptor = DependencyPropertyDescriptor.FromProperty(
                    ItemsSourceProperty, typeof(CustomDataGrid));
                if (_itemsSourceDescriptor != null)
                {
                    // Отписываем предыдущий обработчик (защита от повторного OnApplyTemplate)
                    _itemsSourceDescriptor.RemoveValueChanged(this, OnItemsSourceChangedForPresenter);
                    _itemsSourceDescriptor.AddValueChanged(this, OnItemsSourceChangedForPresenter);
                }
            }

            if (_bottomPanelPresenter != null)
            {
                _bottomPanelPresenter.ParentGrid = this;
                _bottomPanelPresenter.ItemsSource = ItemsSource;
            }

            if (_columnHeadersPresenter != null)
            {
                // Генерируем элементы заголовков, если коллекции уже заполнены
                if (FrozenColumnHeaders.Count > 0 || ScrollableColumnHeaders.Count > 0)
                {
                    _columnHeadersPresenter.GenerateHeaderElements();
                }

                // Отписываем предыдущий обработчик (защита от повторного OnApplyTemplate)
                _columnHeadersPresenter.SizeChanged -= OnColumnHeadersPresenterSizeChanged;

                // Подписываемся на изменение размеров ColumnHeadersPresenter,
                // чтобы синхронизировать ширину spacer-бордюров под скроллбаром
                _columnHeadersPresenter.SizeChanged += OnColumnHeadersPresenterSizeChanged;
            }

            // Первоначальная синхронизация ширины spacer-бордюров
            UpdateScrollBarSpacers();

            // Подписываемся на изменение размеров frozen-панелей в шапке,
            // чтобы синхронизировать ширину spacer-бордюров под скроллбаром
            // при ресайзе колонок (ширина frozen-панелей меняется, а общий
            // размер ColumnHeadersPresenter может не измениться).
            // Используем Dispatcher для поиска после применения шаблона ColumnHeadersPresenter.
            // Отменяем предыдущую операцию, чтобы избежать накопления очереди при повторном OnApplyTemplate.
            // Отписываем старые обработчики СИНХРОННО, до создания новой операции,
            // чтобы гарантировать отписку даже если новая операция не выполнится
            // (например, при выгрузке элемента до выполнения Dispatcher.BeginInvoke).
            if (_leftFrozenBorder != null)
                _leftFrozenBorder.SizeChanged -= OnFrozenBorderSizeChanged;
            if (_rightFrozenBorder != null)
                _rightFrozenBorder.SizeChanged -= OnFrozenBorderSizeChanged;
            _leftFrozenBorder = null;
            _rightFrozenBorder = null;

            if (_frozenBorderUpdateOperation != null)
            {
                _frozenBorderUpdateOperation.Abort();
            }
            _frozenBorderUpdateOperation = Dispatcher.BeginInvoke(new Action(() =>
            {
                _frozenBorderUpdateOperation = null;

                _leftFrozenBorder = FindVisualChildByName<Border>(_columnHeadersPresenter, "PART_LeftFrozenBorder");
                _rightFrozenBorder = FindVisualChildByName<Border>(_columnHeadersPresenter, "PART_RightFrozenBorder");

                if (_leftFrozenBorder != null)
                    _leftFrozenBorder.SizeChanged += OnFrozenBorderSizeChanged;
                if (_rightFrozenBorder != null)
                    _rightFrozenBorder.SizeChanged += OnFrozenBorderSizeChanged;
            }), System.Windows.Threading.DispatcherPriority.Loaded);

            // Отписываем предыдущий обработчик (защита от повторного OnApplyTemplate)
            SizeChanged -= OnCustomDataGridSizeChanged;
            SizeChanged += OnCustomDataGridSizeChanged;

            UpdateScrollBarVisibility();
        }

        private void OnColumnHeadersPresenterSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateScrollBarSpacers();
        }

        /// <summary>
        /// Вызывается при изменении размеров левой или правой frozen-панели в шапке.
        /// Синхронизирует ширину spacer-бордюров под скроллбаром.
        /// </summary>
        private void OnFrozenBorderSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateScrollBarSpacers();
        }

        /// <summary>
        /// Синхронизирует ширину spacer-бордюров под скроллбаром с шириной frozen-панелей в шапке.
        /// Использует сохранённые ссылки _leftFrozenBorder/_rightFrozenBorder, которые
        /// обновляются в OnApplyTemplate. Если ссылки отсутствуют (шаблон ещё не применён),
        /// выполняет поиск по визуальному дереву.
        /// </summary>
        private void UpdateScrollBarSpacers()
        {
            if (_columnHeadersPresenter == null) return;

            // Используем сохранённые ссылки, если они есть, иначе ищем по визуальному дереву
            var leftFrozenBorder = _leftFrozenBorder
                ?? FindVisualChildByName<Border>(_columnHeadersPresenter, "PART_LeftFrozenBorder");
            var rightFrozenBorder = _rightFrozenBorder
                ?? FindVisualChildByName<Border>(_columnHeadersPresenter, "PART_RightFrozenBorder");

            if (_leftScrollBarSpacer != null && leftFrozenBorder != null)
            {
                _leftScrollBarSpacer.Width = leftFrozenBorder.ActualWidth + SeparatorWidth;
            }

            if (_rightScrollBarSpacer != null && rightFrozenBorder != null)
            {
                _rightScrollBarSpacer.Width = rightFrozenBorder.ActualWidth + SeparatorWidth;
            }

        }

        /// <summary>
        /// Ищет дочерний элемент заданного типа по имени в визуальном дереве.
        /// </summary>
        private static T FindVisualChildByName<T>(DependencyObject parent, string name) where T : FrameworkElement
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T fe && fe.Name == name)
                    return fe;

                var result = FindVisualChildByName<T>(child, name);
                if (result != null)
                    return result;
            }
            return null;
        }

        private void OnItemsSourceChangedForPresenter(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"[CustomDataGrid] OnItemsSourceChangedForPresenter: ItemsSource={ItemsSource?.GetType().Name ?? "null"}");

            // ВАЖНО: SyncColumnsWithHeaders() должен быть вызван до проброса ItemsSource
            // в UnifiedRowsPresenter, чтобы DataGrid.Columns были созданы с правильными
            // Binding к свойствам модели. При XAML-парсинге заголовки создаются,
            // но SyncColumnsWithHeaders() вызывается только через OnHeaderCollectionChanged
            // с debounce, и может не успеть выполниться до установки ItemsSource.
            // Без этого вызова колонки не будут отображать данные, даже если заголовки видны.
            if (ItemsSource != null)
            {
                SyncColumnsWithHeaders();
            }

            if (_unifiedRowsPresenter != null)
            {
                _unifiedRowsPresenter.ItemsSource = ItemsSource;
            }

            if (_bottomPanelPresenter != null)
            {
                _bottomPanelPresenter.ItemsSource = ItemsSource;
                _bottomPanelPresenter.UpdateAggregates();
            }
        }

        private void OnCustomDataGridSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateScrollBarViewportSize();
        }

        private void UpdateScrollBarViewportSize()
        {
            if (_horizontalScrollBar == null || Columns == null) return;

            double leftWidth = 0;
            int leftCount = Math.Min(LeftFrozenColumnsCount, Columns.Count);
            for (int i = 0; i < leftCount; i++)
                leftWidth += Columns[i].ActualWidth;

            double rightWidth = 0;
            int rightCount = Math.Min(RightFrozenColumnsCount, Columns.Count);
            for (int i = Columns.Count - rightCount; i < Columns.Count; i++)
                rightWidth += Columns[i].ActualWidth;

            // Вычитаем ширину разделителей (SeparatorWidth * 2), так как CenterColDef
            // в RowTemplates.xaml имеет Width="*" и занимает:
            // MainGrid.ActualWidth - LeftColDef - Separator - RightColDef - Separator
            double separatorWidth = SeparatorWidth;
            double viewportWidth = Math.Max(0, ActualWidth - leftWidth - rightWidth - separatorWidth * 2);
            _horizontalScrollBar.ViewportSize = viewportWidth;
            _horizontalScrollBar.LargeChange = viewportWidth;
        }

        public void UpdateScrollBarVisibility()
        {
            bool isVerticalScrollBarVisible = _unifiedRowsPresenter != null && _unifiedRowsPresenter.VerticalScrollBarWidth > 0.01;
            bool isHorizontalScrollBarVisible = _unifiedRowsPresenter != null && _unifiedRowsPresenter.MaxHorizontalOffset > 0.01;

            if (isVerticalScrollBarVisible)
            {
                var scrollBarWidth = SystemParameters.VerticalScrollBarWidth;
                RightMargin = new Thickness(0, 0, scrollBarWidth, 0);
                LeftMargin = new Thickness(0);
            }
            else
            {
                RightMargin = new Thickness(0);
                LeftMargin = new Thickness(0);
            }
            _bottomPanelPresenter?.UpdateRowWidth();
            if (isHorizontalScrollBarVisible)
            {
                var scrollBarHeight = SystemParameters.HorizontalScrollBarHeight;
                RightBottomMargin = new Thickness(0, 0, 0, scrollBarHeight);
                LeftBottomMargin = new Thickness(0, 0, 0, scrollBarHeight);
            }
            else
            {
                RightBottomMargin = new Thickness(0);
                LeftBottomMargin = new Thickness(0);
            }
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // В режиме Multiple не синхронизируем выделение через OnSelectionChanged,
            // так как управление ведётся через _selectedItems в HandleMultipleSelectionClick.
            // Игнорируем также изменения, инициированные самим DataGrid (например, при сортировке).
            if (RowSelectionMode == RowSelectionMode.Multiple)
                return;

            SyncRowSelection();
        }

    }
}

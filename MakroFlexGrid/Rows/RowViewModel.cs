using MakroFlexGrid.Core;
using MakroFlexGrid.Headers;
using MakroFlexGrid.Utilities;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace MakroFlexGrid.Rows
{
    public class RowViewModel : INotifyPropertyChanged, IDisposable
    {
        private object _item;
        private readonly UnifiedRowsPresenter _presenter;
        private bool _disposed;
        private bool _isSelected;
        private double _horizontalOffset;
        private Brush _gridLineBrush;
        private Brush _leftPanelBackground;
        private Brush _centerPanelBackground;
        private Brush _rightPanelBackground;
        private Brush _rowBackground;
        private Brush _rowSelectedBackground;
        private double _separatorWidth;
        private Brush _separatorBrush;
        private double _rowWidth;
        private DataTemplate _rowDetailsTemplate;
        private DataTemplate _bottomRowTemplate;
        private bool _isSubscribedToRowWidthEvents;
        private bool _isCellSelectionEnabled;
        private Brush _cellSelectedBorderBrush;
        private Thickness _cellSelectedBorderThickness;
        private int _leftFrozenColumnsCount;
        private int _rightFrozenColumnsCount;

        // Слабая ссылка на ScrollManager, чтобы он не удерживал RowViewModel от GC
        private Action<double> _weakScrollManagerHandler;
        private WeakDependencyPropertyListener _scrollBarWidthListener;
        private WeakDependencyPropertyListener _gridSizeListener;

        /// <summary>
        /// Событие, уведомляющее CellViewModel об изменении Item.
        /// Позволяет CellViewModel обновить Value при изменении данных строки.
        /// </summary>
        public event Action<object> ItemChanged;

        public double HorizontalOffset
        {
            get => _horizontalOffset;
            set
            {
                if (_horizontalOffset != value)
                {
                    _horizontalOffset = value;
                    OnPropertyChanged();
                }
            }
        }

        public Brush GridLineBrush
        {
            get => _gridLineBrush;
            set
            {
                if (_gridLineBrush != value)
                {
                    _gridLineBrush = value;
                    OnPropertyChanged();
                }
            }
        }

        public Brush LeftPanelBackground
        {
            get => _leftPanelBackground;
            set
            {
                if (_leftPanelBackground != value)
                {
                    _leftPanelBackground = value;
                    OnPropertyChanged();
                }
            }
        }

        public Brush CenterPanelBackground
        {
            get => _centerPanelBackground;
            set
            {
                if (_centerPanelBackground != value)
                {
                    _centerPanelBackground = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _rightFrozenWidth;
        public double RightFrozenWidth
        {
            get => _rightFrozenWidth;
            set
            {
                if (Math.Abs(_rightFrozenWidth - value) > 0.01)
                {
                    _rightFrozenWidth = value;
                    OnPropertyChanged();
                }
            }
        }

        public Brush RightPanelBackground
        {
            get => _rightPanelBackground;
            set
            {
                if (_rightPanelBackground != value)
                {
                    _rightPanelBackground = value;
                    OnPropertyChanged();
                }
            }
        }

        public Brush RowBackground
        {
            get => _rowBackground;
            set
            {
                if (_rowBackground != value)
                {
                    _rowBackground = value;
                    OnPropertyChanged();
                }
            }
        }

        public Brush RowSelectedBackground
        {
            get => _rowSelectedBackground;
            set
            {
                if (_rowSelectedBackground != value)
                {
                    _rowSelectedBackground = value;
                    OnPropertyChanged();
                }
            }
        }

        public double SeparatorWidth
        {
            get => _separatorWidth;
            set
            {
                if (Math.Abs(_separatorWidth - value) > 0.01)
                {
                    _separatorWidth = value;
                    OnPropertyChanged();
                }
            }
        }

        public Brush SeparatorBrush
        {
            get => _separatorBrush;
            set
            {
                if (_separatorBrush != value)
                {
                    _separatorBrush = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Количество замороженных колонок слева.
        /// Используется для управления видимостью левого разделителя.
        /// </summary>
        public int LeftFrozenColumnsCount
        {
            get => _leftFrozenColumnsCount;
            set
            {
                if (_leftFrozenColumnsCount != value)
                {
                    _leftFrozenColumnsCount = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Количество замороженных колонок справа.
        /// Используется для управления видимостью правого разделителя.
        /// </summary>
        public int RightFrozenColumnsCount
        {
            get => _rightFrozenColumnsCount;
            set
            {
                if (_rightFrozenColumnsCount != value)
                {
                    _rightFrozenColumnsCount = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Включена ли обводка выбранных ячеек.
        /// Синхронизируется из CustomDataGrid.IsCellSelectionEnabled.
        /// </summary>
        public bool IsCellSelectionEnabled
        {
            get => _isCellSelectionEnabled;
            set
            {
                if (_isCellSelectionEnabled != value)
                {
                    _isCellSelectionEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Кисть для обводки выбранных ячеек.
        /// Синхронизируется из CustomDataGrid.CellSelectedBorderBrush.
        /// </summary>
        public Brush CellSelectedBorderBrush
        {
            get => _cellSelectedBorderBrush;
            set
            {
                if (_cellSelectedBorderBrush != value)
                {
                    _cellSelectedBorderBrush = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Толщина обводки выбранных ячеек.
        /// Синхронизируется из CustomDataGrid.CellSelectedBorderThickness.
        /// </summary>
        public Thickness CellSelectedBorderThickness
        {
            get => _cellSelectedBorderThickness;
            set
            {
                if (_cellSelectedBorderThickness != value)
                {
                    _cellSelectedBorderThickness = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Ширина строки. Используется для синхронизации ширины
        /// с горизонтальным скроллом (если он есть). Устанавливается через UnifiedRowsPresenter
        /// при изменении размеров окна или появлении скролла.
        /// </summary>
        public double RowWidth
        {
            get => _rowWidth;
            set
            {

                _rowWidth = value;
                OnPropertyChanged();

            }
        }

        public DataTemplate RowDetailsTemplate
        {
            get => _rowDetailsTemplate;
            set
            {
                if (_rowDetailsTemplate != value)
                {
                    _rowDetailsTemplate = value;
                    OnPropertyChanged();
                }
            }
        }

        public DataTemplate BottomRowTemplate
        {
            get => _bottomRowTemplate;
            set
            {
                if (_bottomRowTemplate != value)
                {
                    _bottomRowTemplate = value;
                    OnPropertyChanged();
                }
            }
        }

        public RowViewModel(object item, UnifiedRowsPresenter presenter)
        {
            _item = item;
            _presenter = presenter;
            SyncPropertiesFromGrid();

#if DEBUG
            MemoryDiagnostics.OnRowViewModelCreated();
#endif

            // Подписываемся на изменение горизонтального оффсета через слабую ссылку,
            // чтобы RowViewModel не удерживал GC, если вдруг Dispose() не был вызван.
            // Используем WeakActionHelper с unsubscribeAction для автоматической отписки
            // при сборке GC контекста, чтобы не осталось висячих подписок
            // на ScrollManager.HorizontalOffsetChanged после уничтожения.
            if (_presenter?.ScrollManager != null)
            {
                _weakScrollManagerHandler = WeakActionHelper.CreateWeakAction<double>(
                    this,
                    OnScrollManagerOffsetChanged,
                    handler => _presenter.ScrollManager.HorizontalOffsetChanged -= handler);
                _presenter.ScrollManager.HorizontalOffsetChanged += _weakScrollManagerHandler;
            }
            HorizontalOffset = _presenter.ScrollManager.HorizontalOffset;

            // Подписываемся на события изменения ширины
            SubscribeToRowWidthEvents();
        }

        public UnifiedRowsPresenter Presenter => _presenter;

        public void SyncPropertiesFromGrid()
        {
            var grid = _presenter?.ParentGrid;
            if (grid != null)
            {
                GridLineBrush = grid.GridLineBrush;
                LeftPanelBackground = grid.LeftFrozenPanelBackground;
                CenterPanelBackground = grid.CenterPanelBackground;
                RightPanelBackground = grid.RightFrozenPanelBackground;
                RowBackground = grid.RowBackground;
                RowSelectedBackground = grid.RowSelectedBackground;
                SeparatorWidth = grid.SeparatorWidth;
                SeparatorBrush = grid.SeparatorBrush;
                RowDetailsTemplate = grid.RowDetailsTemplate;
                BottomRowTemplate = grid.BottomRowTemplate;
                IsCellSelectionEnabled = grid.IsCellSelectionEnabled;
                CellSelectedBorderBrush = grid.CellSelectedBorderBrush;
                CellSelectedBorderThickness = grid.CellSelectedBorderThickness;
                LeftFrozenColumnsCount = _presenter.LeftFrozenColumnsCount;
                RightFrozenColumnsCount = _presenter.RightFrozenColumnsCount;
            }
        }

        public object Item
        {
            get => _item;
            set
            {
                if (_item != value)
                {
                    _item = value;
                    OnPropertyChanged();
                    ItemChanged?.Invoke(value);
                }
            }
        }

        public ObservableCollection<CellViewModel> LeftCells { get; } = new ObservableCollection<CellViewModel>();
        public ObservableCollection<CellViewModel> CenterCells { get; } = new ObservableCollection<CellViewModel>();
        public ObservableCollection<CellViewModel> RightCells { get; } = new ObservableCollection<CellViewModel>();

        /// <summary>
        /// Обновить ячейки строки, создав CellViewModel для указанных колонок.
        /// Метод использует существующие ViewModel где возможно,
        /// переиспользуя их (чтобы избежать лишних TranslateTransform и подписок).
        /// </summary>
        public void UpdateCells(
    IEnumerable<DataGridColumn> leftColumns,
    IEnumerable<DataGridColumn> centerColumns,
    IEnumerable<DataGridColumn> rightColumns)
        {
            var grid = _presenter?.ParentGrid;

            // Оптимизированное обновление для каждой панели:
            // сначала собираем словарь существующих CellViewModel по колонкам в этой панели,
            // затем проходим по новым колонкам и переиспользуем существующие.
            // Так мы избегаем создания/удаления лишних ячеек.
            UpdateCellCollection(LeftCells, leftColumns, grid, isLeftPanel: true);
            UpdateCellCollection(CenterCells, centerColumns, grid, isLeftPanel: false);
            UpdateCellCollection(RightCells, rightColumns, grid, isLeftPanel: false);

            // Отмечаем (первую ячейку) правой frozen-панели как крайнюю слева
            if (RightCells.Count > 0)
                RightCells[0].IsLeftmostInRightPanel = true;
        }

        /// <summary>
        /// Внутреннее обновление коллекции ячеек: переиспользует старые, удаляет лишние,
        /// добавляет новые в правильном порядке.
        /// Также учитывает системную колонку (SystemCellViewModel, Column == null),
        /// которая должна быть всегда первой.
        /// </summary>
        private void UpdateCellCollection(
            ObservableCollection<CellViewModel> cells,
            IEnumerable<DataGridColumn> newColumns,
            CustomDataGrid grid,
            bool isLeftPanel)
        {
            // Сначала собираем словарь старых ячеек для быстрого поиска
            var existingCells = new Dictionary<DataGridColumn, CellViewModel>();
            CellViewModel systemCell = null;
            foreach (var cell in cells)
            {
                if (cell.Column != null)
                    existingCells[cell.Column] = cell;
                else if (isLeftPanel)
                    systemCell = cell; // системная ячейка (Column == null)
            }

            int index = 0;

            // Системная колонка (если включена) должна быть всегда первой в левой панели
            bool systemColumnEnabled = grid != null && grid.IsSystemColumnEnabled;
            if (isLeftPanel && systemColumnEnabled)
            {
                if (systemCell == null)
                {
                    // Создаем системную ячейку и вставляем в начало
                    systemCell = new SystemCellViewModel(this);
                    cells.Insert(0, systemCell);
                }
                // Если системная ячейка уже есть, но она не на первой позиции - перемещаем
                else
                {
                    var sysIndex = cells.IndexOf(systemCell);
                    if (sysIndex != 0)
                    {
                        cells.Move(sysIndex, 0);
                    }
                }
                index = 1; // после системной ячейки
            }
            else if (isLeftPanel && !systemColumnEnabled && systemCell != null)
            {
                // Удаляем старую системную ячейку и отписываемся от событий
                systemCell.UnsubscribeFromEvents();
                cells.Remove(systemCell);
            }

            // Проходим по новым колонкам и обновляем/добавляем ячейки
            foreach (var column in newColumns)
            {
                if (existingCells.TryGetValue(column, out var existingCell))
                {
                    // Если ячейка существует и находится не на правильной позиции, перемещаем
                    var currentIndex = cells.IndexOf(existingCell);
                    if (currentIndex != index)
                    {
                        cells.Move(currentIndex, index);
                    }
                    existingCells.Remove(column);
                }
                else
                {
                    // Создаем новую ячейку через фабрику.
                    // Фабрика позволяет использовать специализированные CellViewModel
                    // для разных типов ColumnHeaderLeaf.
                    var headerItem = grid?.GetColumnHeaderItem(column);
                    var newCell = CellViewModelFactory.Create(this, column, headerItem);
                    cells.Insert(index, newCell);
                }
                index++;
            }

            // Удаляем ячейки, которые больше нет в списке колонок
            foreach (var removedCell in existingCells.Values)
            {
                removedCell.UnsubscribeFromEvents();
                cells.Remove(removedCell);
            }
        }

        /// <summary>
        /// Получает уведомление об изменении горизонтального offset из ScrollManager.
        /// Обновляет HorizontalOffset как attached property в шаблоне строки.
        /// </summary>
        private void OnScrollManagerOffsetChanged(double offset)
        {
            HorizontalOffset = offset;
        }

        /// <summary>
        /// Вспомогательный метод: отписывает CellViewModel от событий.
        /// </summary>
        /// <summary>
        /// Подписывается на события, необходимые для вычисления RowWidth:
        /// VerticalScrollBarWidth из UnifiedRowsPresenter и ActualWidth из CustomDataGrid.
        /// </summary>
        private void SubscribeToRowWidthEvents()
        {
            if (_isSubscribedToRowWidthEvents) return;
            _isSubscribedToRowWidthEvents = true;

            if (_presenter != null)
            {
                // Подписываемся на изменение VerticalScrollBarWidth через WeakDependencyPropertyListener.
                // Используем DependencyPropertyDescriptor, так как UnifiedRowsPresenter
                // (наследник FrameworkElement) не реализует INotifyPropertyChanged,
                // и PropertyChangedEventManager не может быть использован.
                // WeakDependencyPropertyListener предотвращает утечки памяти при сборке GC контекста,
                // удерживая слабую ссылку вместо сильной через EventHandlerStore.
                var scrollBarWidthDescriptor = DependencyPropertyDescriptor.FromProperty(
                    UnifiedRowsPresenter.VerticalScrollBarWidthProperty, typeof(UnifiedRowsPresenter));
                if (scrollBarWidthDescriptor != null)
                {
                    _scrollBarWidthListener = new WeakDependencyPropertyListener(
                        scrollBarWidthDescriptor, _presenter, this, OnScrollBarWidthChanged);
                }

                // Подписываемся на изменение ширины грида через WeakDependencyPropertyListener
                // вместо WeakEventManager. WeakEventManager хранит WeakReference на слушателя,
                // но instance-метод OnGridSizeChanged захватывает сильную ссылку на RowViewModel
                // через this, что предотвращает сборку мусора.
                // WeakDependencyPropertyListener решает эту проблему, так как использует
                // WeakReference на получателя и предотвращает утечки при сборке GC.
                if (_presenter.ParentGrid != null)
                {
                    var gridWidthDescriptor = DependencyPropertyDescriptor.FromProperty(
                        FrameworkElement.ActualWidthProperty, typeof(CustomDataGrid));
                    if (gridWidthDescriptor != null)
                    {
                        // WeakDependencyPropertyListener принимает EventHandler (EventArgs).
                        // OnGridSizeChanged имеет сигнатуру (object, SizeChangedEventArgs),
                        // что требует адаптации через лямбда-выражение. Лямбда захватывает this,
                        // но WeakDependencyPropertyListener хранит WeakReference на target (this),
                        // что предотвращает утечку памяти.
                        _gridSizeListener = new WeakDependencyPropertyListener(
                            gridWidthDescriptor, _presenter.ParentGrid, this,
                            OnGridSizeChanged);
                    }
                }
            }

            // Принудительно обновляем ширину сейчас
            UpdateRowWidthFromGrid();
        }

        /// <summary>
        /// Отписывается от событий обновления RowWidth.
        /// </summary>
        private void UnsubscribeFromRowWidthEvents()
        {
            if (!_isSubscribedToRowWidthEvents) return;
            _isSubscribedToRowWidthEvents = false;

            if (_scrollBarWidthListener != null)
            {
                _scrollBarWidthListener.Dispose();
                _scrollBarWidthListener = null;
            }

            if (_gridSizeListener != null)
            {
                _gridSizeListener.Dispose();
                _gridSizeListener = null;
            }
        }

        private void OnScrollBarWidthChanged(object sender, EventArgs e)
        {
            UpdateRowWidthFromGrid();
        }

        private void OnGridSizeChanged(object sender, EventArgs e)
        {
            UpdateRowWidthFromGrid();
        }

        /// <summary>
        /// Вычисляет RowWidth на основе ширины грида и ширины скролла.
        /// </summary>
        private void UpdateRowWidthFromGrid()
        {
            if (_presenter?.ParentGrid == null) return;
            double gridWidth = _presenter.ParentGrid.ActualWidth;
            double scrollBarWidth = _presenter.VerticalScrollBarWidth;
            RowWidth = Math.Max(0, gridWidth - scrollBarWidth - 2);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

#if DEBUG
            MemoryDiagnostics.OnRowViewModelDisposed();
#endif

            // Отписываемся от ScrollManager через слабую ссылку
            if (_presenter?.ScrollManager != null && _weakScrollManagerHandler != null)
            {
                _presenter.ScrollManager.HorizontalOffsetChanged -= _weakScrollManagerHandler;
            }

            // Отписываемся от событий ширины строки
            UnsubscribeFromRowWidthEvents();

            // Отписываем CellViewModel от событий перед очисткой коллекции.
            // Важно: UnsubscribeFromEvents() удаляет WeakDependencyPropertyListener
            // из внутреннего EventHandlerStore WPF, что предотвращает утечки памяти.
            foreach (var cell in LeftCells)
                cell.UnsubscribeFromEvents();
            foreach (var cell in CenterCells)
                cell.UnsubscribeFromEvents();
            foreach (var cell in RightCells)
                cell.UnsubscribeFromEvents();

            // Очищаем коллекции, чтобы ListCollectionView (внутри ItemCollection
            // в ItemsControl грида) отписался от ObservableCollection через CollectionChanged.
            // Иначе ListCollectionView хранит сильную ссылку на ObservableCollection,
            // которая держит instance-методы RowViewModel, предотвращая сборку мусора.
            LeftCells.Clear();
            CenterCells.Clear();
            RightCells.Clear();

            // Сбрасываем Item, чтобы разорвать цепочку сильных ссылок на данные
            // через подписку на ItemChanged (если WeakActionHelper не сработал).
            _item = null;

            _weakScrollManagerHandler = null;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class CellViewModel : INotifyPropertyChanged
    {
        // Используем ConditionalWeakTable для PropertyInfo, чтобы не хранить сильные ссылки на типы.
        // Используем ConditionalWeakTable вместо ConcurrentDictionary, чтобы сборщик мусора
        // мог автоматически удалять записи для типов, которые больше не используются.
        // ConcurrentDictionary с ключом (Type, string) не подходит для этой цели
        // и создавал бы утечки памяти.
        private static readonly ConditionalWeakTable<Type, Dictionary<string, PropertyInfo>> _globalPropertyCache
            = new ConditionalWeakTable<Type, Dictionary<string, PropertyInfo>>();

        // Храним слабую ссылку на RowViewModel, чтобы не препятствовать сборке мусора.
        // CellViewModel может пережить RowViewModel при утилизации, и сильная ссылка
        // предотвращала бы GC RowViewModel. WeakReference решает эту проблему.
        private readonly WeakReference<RowViewModel> _weakRowViewModel;
        private readonly DataGridColumn _column;
        private Func<object, object> _valueGetter;
        private double _width;
        private bool _isSubscribedToWidth;
        private bool _isLeftmostInRightPanel;
        private bool _isEditing;
        private string _editValue;
        private bool _isCellSelected;

        // Слабые подписки
        private WeakDependencyPropertyListener _columnWidthListener;
        private Action<object> _weakItemChangedHandler;

        /// <summary>
        /// Тип ячейки. Используется в UnifiedCellTemplate для выбора визуального представления.
        /// Заполняется из ColumnHeaderLeaf через ApplyHeaderConfig().
        /// </summary>
        private string _cellType;

        /// <summary>
        /// Тип ячейки (Editable, ComboBox, Numeric, CheckBox и т.д.).
        /// Используется в UnifiedCellTemplate.xaml для выбора DataTemplate через DataTrigger.
        /// </summary>
        public string CellType
        {
            get => _cellType;
            set
            {
                if (_cellType != value)
                {
                    _cellType = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Словарь конфигурации ячейки.
        /// Заполняется из ColumnHeaderLeaf через ApplyHeaderConfig().
        /// Позволяет XAML-шаблонам получать параметры через Config[Key].
        /// Заменяет необходимость добавлять отдельные свойства для каждого типа ячейки.
        /// </summary>
        private Dictionary<string, object> _config = new Dictionary<string, object>();

        /// <summary>
        /// Доступ к конфигурации ячейки по ключу.
        /// Используется в XAML: {Binding Config[KeyName]}.
        /// </summary>
        public Dictionary<string, object> Config => _config;

        /// <summary>
        /// Получает значение конфигурации по ключу с приведением типа.
        /// Удобно для использования в C#-коде (например, в behavior).
        /// </summary>
        public T GetConfig<T>(string key, T defaultValue = default)
        {
            if (_config.TryGetValue(key, out var value) && value is T typedValue)
                return typedValue;
            return defaultValue;
        }

        /// <summary>
        /// Получает значение конфигурации по ключу (без приведения типа).
        /// </summary>
        public object GetConfig(string key)
        {
            _config.TryGetValue(key, out var value);
            return value;
        }

        /// <summary>
        /// Возвращает RowViewModel, если он еще жив. Может вернуть null.
        /// </summary>
        public RowViewModel RowViewModel
        {
            get
            {
                _weakRowViewModel.TryGetTarget(out var target);
                return target;
            }
        }

        /// <summary>
        /// Возвращает Item из RowViewModel, если он еще жив.
        /// </summary>
        public object Item
        {
            get
            {
                if (_weakRowViewModel.TryGetTarget(out var rvm))
                    return rvm.Item;
                return null;
            }
        }

        private DataTemplate _cachedTemplate;

        public DataTemplate Template
        {
            get
            {
                if (_cachedTemplate != null) return _cachedTemplate;

                if (!_weakRowViewModel.TryGetTarget(out var rvm))
                    return null;

                var headerItem = rvm.Presenter?.ParentGrid?.GetColumnHeaderItem(_column);
                _cachedTemplate = headerItem?.CellTemplate;
                return _cachedTemplate;
            }
        }

        public CellViewModel(RowViewModel rowViewModel, DataGridColumn column)
        {
            _weakRowViewModel = new WeakReference<RowViewModel>(rowViewModel ?? throw new ArgumentNullException(nameof(rowViewModel)));
            _column = column;

#if DEBUG
            MemoryDiagnostics.OnCellViewModelCreated();
#endif

            InitializeValueGetter();

            // Предварительно кэшируем шаблон, чтобы избежать рекурсии при обращении через Binding в UI
            if (rowViewModel.Presenter?.ParentGrid != null)
            {
                var headerItem = rowViewModel.Presenter.ParentGrid.GetColumnHeaderItem(column);
                _cachedTemplate = headerItem?.CellTemplate;

                // Заполняем Config из ColumnHeaderLeaf
                if (headerItem is ColumnHeaderLeaf headerLeaf)
                {
                    ApplyHeaderConfig(headerLeaf);
                }
            }

            // Подписываемся на изменение Item для обновления Value через слабую ссылку.
            // Используем WeakActionHelper с unsubscribeAction для автоматической отписки
            // при сборке GC контекста, чтобы не осталось висячих подписок
            // на RowViewModel.ItemChanged после уничтожения.
            _weakItemChangedHandler = WeakActionHelper.CreateWeakAction<object>(
                this,
                OnRowItemChanged,
                handler => rowViewModel.ItemChanged -= handler);
            rowViewModel.ItemChanged += _weakItemChangedHandler;

            // Подписываемся на изменение ширины колонки для обновления _width,
            // чтобы своевременно обновлять визуальную ширину ячейки.
            // Используем ActualWidthProperty, а не WidthProperty, так как именно
            // фактическая ширина ActualWidth нас интересует, а не заданная ширина
            // с возможной отложенной обработкой (DeferredResize).
            SubscribeToColumnWidth();

            // Инициализируем начальную ширину ячейки из колонки или из ColumnHeaderItem, чтобы OnColumnWidthChanged обновил _width.
            if (column != null)
            {
                // Если ActualWidth = 0 (колонка еще не участвовала в layout или скрыта),
                // используем Width из ColumnHeaderItem, если он доступен.
                if (column.ActualWidth > 0)
                {
                    _width = column.ActualWidth;
                }
                else
                {
                    // Используем Width из ColumnHeaderItem, чтобы ширина ячейки не была нулевой
                    // даже при скрытой колонке или в момент перетаскивания (например, при Drag&Drop).
                    var headerItem = rowViewModel.Presenter?.ParentGrid?.GetColumnHeaderItem(column);
                    if (headerItem != null && headerItem.Width > 0)
                    {
                        _width = headerItem.Width;
                    }
                }
            }
        }

        /// <summary>
        /// Заполняет словарь Config из Dependency Properties ColumnHeaderLeaf.
        /// Позволяет XAML-шаблонам получать параметры через Config[Key].
        /// Также устанавливает CellType на основе имени класса заголовка.
        /// </summary>
        private void ApplyHeaderConfig(ColumnHeaderLeaf headerLeaf)
        {
            // Устанавливаем CellType из имени класса (убираем суффикс "ColumnHeaderLeaf")
            var typeName = headerLeaf.GetType().Name;
            CellType = typeName.EndsWith("ColumnHeaderLeaf")
                ? typeName.Substring(0, typeName.Length - "ColumnHeaderLeaf".Length)
                : typeName;

            var leafType = headerLeaf.GetType();
            var dps = leafType.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                .Where(f => f.FieldType == typeof(DependencyProperty))
                .Select(f => f.GetValue(null) as DependencyProperty)
                .Where(dp => dp != null);

            foreach (var dp in dps)
            {
                // Пропускаем стандартные свойства, определённые в ColumnHeaderItem
                if (IsStandardProperty(dp))
                    continue;

                var value = headerLeaf.GetValue(dp);
                if (value != null)
                {
                    _config[dp.Name] = value;
                }
            }
        }

        /// <summary>
        /// Проверяет, является ли DependencyProperty стандартным свойством ColumnHeaderItem.
        /// Такие свойства не копируются в Config, так как они не специфичны для типа ячейки.
        /// </summary>
        private static bool IsStandardProperty(DependencyProperty dp)
        {
            // Используем строковые литералы, так как эти поля объявлены в ColumnHeaderItem,
            // а не в текущем классе, и nameof() не может их разрешить без полной квалификации.
            switch (dp.Name)
            {
                case "Width":
                case "MinWidth":
                case "MaxWidth":
                case "IsVisible":
                case "Header":
                case "HeaderStyle":
                case "HeaderTemplate":
                case "HorizontalHeaderAlignment":
                case "VerticalHeaderAlignment":
                case "CanUserSort":
                case "SortDirection":
                case "SortMemberPath":
                case "SortDataType":
                case "CellTemplate":
                case "BottomCellTemplate":
                case "AggregateType":
                case "AllowDrag":
                case "AllowCrossSectionDrag":
                case "IsDragging":
                case "IsDropTarget":
                case "CanUserFilter":
                case "CanUserHide":
                case "Filter":
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Кэшированный DependencyPropertyDescriptor для DataGridColumn.ActualWidthProperty.
        /// Используем один дескриптор на все колонки, а не создаем для каждой CellViewModel.
        /// </summary>
        private static readonly DependencyPropertyDescriptor ActualWidthDescriptor =
            DependencyPropertyDescriptor.FromProperty(
                DataGridColumn.ActualWidthProperty, typeof(DataGridColumn));

        private void SubscribeToColumnWidth()
        {
            if (_column == null) return;
            if (_isSubscribedToWidth) return;
            _isSubscribedToWidth = true;

            // Подписываемся на ActualWidthProperty через WeakDependencyPropertyListener.
            // DataGridColumn не реализует INotifyPropertyChanged, поэтому
            // PropertyChangedEventManager не может быть использован.
            // DependencyPropertyDescriptor - это стандартный способ подписки
            // на изменения DependencyProperty для любых объектов.
            // WeakDependencyPropertyListener предотвращает утечки памяти при сборке GC контекста,
            // удерживая слабую ссылку вместо сильной через EventHandlerStore.
            if (ActualWidthDescriptor != null)
            {
                _columnWidthListener = new WeakDependencyPropertyListener(
                    ActualWidthDescriptor, _column, this, OnColumnWidthChanged);
            }
        }

        private void UnsubscribeFromColumnWidth()
        {
            if (!_isSubscribedToWidth) return;
            _isSubscribedToWidth = false;

            if (_columnWidthListener != null)
            {
                _columnWidthListener.Dispose();
                _columnWidthListener = null;
            }
        }

        private void OnColumnWidthChanged(object sender, EventArgs e)
        {
            Width = _column.ActualWidth;
        }

        private void InitializeValueGetter()
        {
            if (_column is DataGridBoundColumn boundColumn && boundColumn.Binding is Binding binding)
            {
                var path = binding.Path.Path;

                // Используем кэширование PropertyInfo через ConditionalWeakTable, чтобы не создавать
                // дублирующие записи для каждой CellViewModel. ConditionalWeakTable гарантирует,
                // что записи для типа (ключа) будут автоматически удалены при сборке мусора.
                _valueGetter = (item) =>
                {
                    if (item == null) return null;
                    var type = item.GetType();
                    if (!_globalPropertyCache.TryGetValue(type, out var propertyDict))
                    {
                        propertyDict = new Dictionary<string, PropertyInfo>();
                        _globalPropertyCache.Add(type, propertyDict);
                    }
                    if (!propertyDict.TryGetValue(path, out var property))
                    {
                        property = type.GetProperty(path);
                        propertyDict[path] = property;
                    }
                    return property?.GetValue(item);
                };
            }
        }

        public DataGridColumn Column => _column;

        public string Value
        {
            get
            {
                if (!_weakRowViewModel.TryGetTarget(out var rvm) || rvm.Item == null || _valueGetter == null)
                    return "";

                try
                {
                    var value = _valueGetter(rvm.Item);
                    if (value is decimal)
                        return ((decimal)value).ToString("0.00");

                    return value?.ToString() ?? "";
                }
                catch
                {
                    return "";
                }
            }
        }

        /// <summary>
        /// Ширина ячейки. Синхронизируется со значением из DataGridColumn.ActualWidthProperty.
        /// Используется визуальным слоем для установки ширины (через Binding CellViewModel),
        /// что избавляет от необходимости вызывать Dispose() вручную.
        /// </summary>
        /// <summary>
        /// True, если эта ячейка самая левая в правой frozen-панели.
        /// В этом случае визуальный слой рисует разделитель слева от этой ячейки.
        /// </summary>
        public bool IsLeftmostInRightPanel
        {
            get => _isLeftmostInRightPanel;
            set
            {
                if (_isLeftmostInRightPanel != value)
                {
                    _isLeftmostInRightPanel = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// True, если ячейка находится в выбранной строке и включена обводка ячеек.
        /// Используется в RowTemplate.xaml для отображения внутренней обводки.
        /// </summary>
        public bool IsCellSelected
        {
            get => _isCellSelected;
            set
            {
                if (_isCellSelected != value)
                {
                    _isCellSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        public double Width
        {
            get => _width;
            set
            {
                if (Math.Abs(_width - value) > 0.01)
                {
                    _width = value;
                    OnPropertyChanged();
                }
            }
        }

        #region Editing Support

        /// <summary>
        /// True, если ячейка находится в режиме редактирования.
        /// Используется в EditableCellTemplate.xaml для переключения видимости TextBlock/TextBox.
        /// </summary>
        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                if (_isEditing != value)
                {
                    _isEditing = value;
                    OnPropertyChanged();

                    if (value)
                    {
                        // При входе в режим редактирования сохраняем текущее значение
                        _editValue = Value;
                        OnPropertyChanged(nameof(EditValue));
                    }
                }
            }
        }

        /// <summary>
        /// Значение для редактирования. Используется для TwoWay-привязки TextBox в EditableCellTemplate.
        /// </summary>
        public string EditValue
        {
            get => _editValue;
            set
            {
                if (_editValue != value)
                {
                    _editValue = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Сохраняет отредактированное значение в модель данных.
        /// Вызывается из EditableCellBehavior при Enter или LostFocus.
        /// </summary>
        public void CommitEdit()
        {
            if (!_weakRowViewModel.TryGetTarget(out var rvm) || rvm.Item == null)
            {
                IsEditing = false;
                return;
            }

            // Записываем отредактированное значение обратно в Item
            if (_column is DataGridBoundColumn boundColumn && boundColumn.Binding is Binding binding)
            {
                var path = binding.Path.Path;
                var item = rvm.Item;
                var type = item.GetType();
                var property = type.GetProperty(path);
                if (property != null && property.CanWrite)
                {
                    // Пытаемся сконвертировать строку в целевой тип
                    var targetType = property.PropertyType;
                    object convertedValue = _editValue;
                    if (targetType == typeof(int) && int.TryParse(_editValue, out var intVal))
                        convertedValue = intVal;
                    else if (targetType == typeof(long) && long.TryParse(_editValue, out var longVal))
                        convertedValue = longVal;
                    else if (targetType == typeof(decimal) && decimal.TryParse(_editValue, out var decVal))
                        convertedValue = decVal;
                    else if (targetType == typeof(double) && double.TryParse(_editValue, out var dblVal))
                        convertedValue = dblVal;
                    else if (targetType == typeof(float) && float.TryParse(_editValue, out var floatVal))
                        convertedValue = floatVal;
                    else if (targetType == typeof(DateTime) && System.DateTime.TryParse(_editValue, out var dtVal))
                        convertedValue = dtVal;
                    else if (targetType == typeof(bool) && bool.TryParse(_editValue, out var boolVal))
                        convertedValue = boolVal;

                    property.SetValue(item, convertedValue);
                }
            }

            IsEditing = false;
            OnPropertyChanged(nameof(Value));
        }

        /// <summary>
        /// Отменяет редактирование и восстанавливает исходное значение.
        /// Вызывается из EditableCellBehavior при Escape.
        /// </summary>
        public void CancelEdit()
        {
            _editValue = Value;
            OnPropertyChanged(nameof(EditValue));
            IsEditing = false;
        }

        #endregion

        #region Cell Type Detection (for template selection)

        /// <summary>
        /// True, если колонка является EditableColumnHeaderLeaf.
        /// Используется в триггерах RowTemplate.xaml для выбора EditableCellTemplate.
        /// </summary>
        public bool IsEditable
        {
            get
            {
                if (!_weakRowViewModel.TryGetTarget(out var rvm))
                    return false;
                var headerItem = rvm.Presenter?.ParentGrid?.GetColumnHeaderItem(_column);
                return headerItem is EditableColumnHeaderLeaf;
            }
        }

        /// <summary>
        /// Источник данных для ComboBox (из ComboBoxColumnHeaderLeaf.ItemsSource).
        /// </summary>
        public IEnumerable ItemsSource
        {
            get
            {
                if (!_weakRowViewModel.TryGetTarget(out var rvm))
                    return null;
                var headerItem = rvm.Presenter?.ParentGrid?.GetColumnHeaderItem(_column);
                return (headerItem as ComboBoxColumnHeaderLeaf)?.ItemsSource;
            }
        }

        /// <summary>
        /// Путь к отображаемому свойству для ComboBox.
        /// </summary>
        public string ComboBoxDisplayMemberPath
        {
            get
            {
                if (!_weakRowViewModel.TryGetTarget(out var rvm))
                    return null;
                var headerItem = rvm.Presenter?.ParentGrid?.GetColumnHeaderItem(_column);
                return (headerItem as ComboBoxColumnHeaderLeaf)?.DisplayMemberPath;
            }
        }

        /// <summary>
        /// Путь к свойству значения для ComboBox.
        /// </summary>
        public string ComboBoxSelectedValuePath
        {
            get
            {
                if (!_weakRowViewModel.TryGetTarget(out var rvm))
                    return null;
                var headerItem = rvm.Presenter?.ParentGrid?.GetColumnHeaderItem(_column);
                return (headerItem as ComboBoxColumnHeaderLeaf)?.SelectedValuePath;
            }
        }

        /// <summary>
        /// Путь к свойству в Item для привязки SelectedValue ComboBox.
        /// </summary>
        public string ComboBoxSelectedValueBinding
        {
            get
            {
                if (!_weakRowViewModel.TryGetTarget(out var rvm))
                    return null;
                var headerItem = rvm.Presenter?.ParentGrid?.GetColumnHeaderItem(_column);
                return (headerItem as ComboBoxColumnHeaderLeaf)?.SelectedValueBinding;
            }
        }

        #endregion

        private void OnRowItemChanged(object newItem)
        {
            // При изменении Item обновляем Value, чтобы триггернуть в XAML обновление привязки
            OnPropertyChanged(nameof(Value));
        }

        /// <summary>
        /// Отписывается от событий. Вызывается из RowViewModel.Dispose().
        /// </summary>
        public void UnsubscribeFromEvents()
        {
            UnsubscribeFromColumnWidth();

            // Отписываемся от события ItemChanged через слабую ссылку.
            // Если RowViewModel уже собран GC, пропускаем отписку -
            // WeakActionHelper сам обработает этот случай.
            if (_weakItemChangedHandler != null)
            {
                if (_weakRowViewModel.TryGetTarget(out var rvm))
                {
                    rvm.ItemChanged -= _weakItemChangedHandler;
                }
            }

            _weakItemChangedHandler = null;

#if DEBUG
            MemoryDiagnostics.OnCellViewModelDisposed();
#endif
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

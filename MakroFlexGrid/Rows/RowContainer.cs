using MakroFlexGrid.FrozenSeparators;
using MakroFlexGrid.Headers;
using MakroFlexGrid.Utilities;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace MakroFlexGrid.Rows
{
    /// <summary>
    /// Контейнер для строки. Синхронизация горизонтального скролла
    /// выполняется через ScrollHelper.HorizontalOffset attached property в XAML,
    /// который привязан к RowViewModel.HorizontalOffset.
    /// </summary>
    public class RowContainer : ContentPresenter
    {
        public event Action<RowViewModel> RowClicked;
        public event Action<RowViewModel> RowDoubleClicked;

        /// <summary>
        /// Событие возникает при клике правой кнопкой мыши по ячейке.
        /// Передаёт RowViewModel строки и ColumnHeaderItem колонки.
        /// </summary>
        public event Action<RowViewModel, ColumnHeaderItem> CellRightClicked;

        /// <summary>
        /// Событие возникает при клике левой кнопкой мыши по ячейке.
        /// Передаёт CellViewModel ячейки, по которой кликнули.
        /// </summary>
        public event Action<CellViewModel> CellClicked;

        // Храним ссылки на делегаты для возможности отписки при переиспользовании контейнера
        private Action<RowViewModel> _rowClickedHandler;
        private Action<RowViewModel> _rowDoubleClickedHandler;
        private Action<RowViewModel, ColumnHeaderItem> _cellRightClickedHandler;
        private Action<CellViewModel> _cellClickedHandler;

        // CRITICAL: Сохраняем старую RowViewModel из DataContextChanged для Dispose().
        // WPF при VirtualizationMode.Recycling сбрасывает DataContext в null ДО вызова
        // PrepareContainerForItemOverride, поэтому DataContext в Clear() уже недоступен.
        // _lastVm перехватывает VM в момент сброса и используется как fallback.
        private RowViewModel _lastVm;

        public RowContainer()
        {
            // Подписываемся на изменение DataContext, чтобы сохранять старую VM
            // для Dispose() в случаях, когда Clear() вызывается без forceVm
            // (например, из OnCleanUpVirtualizedItem или ForceCleanAllContainers).
            DataContextChanged += OnDataContextChanged;

#if DEBUG
            MemoryDiagnostics.OnRowContainerCreated();
#endif
        }

        /// <summary>
        /// Сохраняет старую RowViewModel в _lastVm для Dispose() в Clear().
        /// WPF при VirtualizationMode.Recycling сбрасывает DataContext в null
        /// ДО вызова PrepareContainerForItemOverride, поэтому DataContext в Clear()
        /// уже недоступен. _lastVm перехватывает VM в момент сброса.
        /// </summary>
        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // WPF сбрасывает DataContext в null при переиспользовании контейнера.
            // Сохраняем старую VM в _lastVm, чтобы Clear() мог её диспозить.
            var oldVm = e.OldValue as RowViewModel;
            if (oldVm != null)
            {
                _lastVm = oldVm;
            }
        }

        /// <summary>
        /// Гарантирует подписку на PreviewMouseDown.
        /// Вызывается после Clear() при переиспользовании контейнера (виртуализация),
        /// так как Clear() отписывает обработчик.
        /// </summary>
        public void EnsureSubscribed()
        {
            PreviewMouseDown -= OnPreviewMouseDown;
            PreviewMouseDown += OnPreviewMouseDown;

            // ВОССТАНАВЛИВАЕМ подписку на DataContextChanged
            DataContextChanged -= OnDataContextChanged;
            DataContextChanged += OnDataContextChanged;
        }

        private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!(Content is RowViewModel vm))
                return;

            // Проверяем, не является ли источник события RowSeparatorGripper (Thumb).
            // Если пользователь хватается за Gripper для ресайза frozen-панели,
            // не нужно выделять строку — Gripper сам обрабатывает drag.
            if (IsMouseOverGripper(e.OriginalSource as DependencyObject))
                return;

            // Обработка правого клика по ячейке
            if (e.ChangedButton == MouseButton.Right && e.ClickCount == 1)
            {
                var cellVm = FindCellViewModel(e.OriginalSource as DependencyObject);
                if (cellVm != null && cellVm.Column != null)
                {
                    var headerItem = GetColumnHeaderForCell(cellVm);
                    if (headerItem != null)
                    {
                        CellRightClicked?.Invoke(vm, headerItem);
                        e.Handled = true;
                        return;
                    }
                }
            }

            // Левый клик по ячейке — определяем CellViewModel и вызываем CellClicked.
            // Если клик по системной ячейке (Column == null), не вызываем CellClicked,
            // а позволяем выполниться RowClicked, чтобы строка выделилась как обычно.
            // Важно: НЕ вызываем RowClicked, если клик был по data-ячейке, чтобы
            // CustomDataGrid.OnRowClicked не сбросил выделение ячейки.
            if (e.ChangedButton == MouseButton.Left && e.ClickCount == 1)
            {
                var cellVm = FindCellViewModel(e.OriginalSource as DependencyObject);
                if (cellVm != null && cellVm.Column != null)
                {
                    CellClicked?.Invoke(cellVm);
                    return; // Не вызываем RowClicked
                }
            }

            if (e.ClickCount == 2)
            {
                RowDoubleClicked?.Invoke(vm);
            }
            else
            {
                RowClicked?.Invoke(vm);
            }
        }

        /// <summary>
        /// Поднимается по визуальному дереву от originalSource до RowContainer
        /// и ищет элемент, у которого DataContext является CellViewModel.
        /// </summary>
        private static CellViewModel FindCellViewModel(DependencyObject originalSource)
        {
            if (originalSource == null) return null;

            DependencyObject current = originalSource;
            while (current != null && !(current is RowContainer))
            {
                if (current is FrameworkElement fe && fe.DataContext is CellViewModel cellVm)
                    return cellVm;
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }

        /// <summary>
        /// Получает ColumnHeaderItem для CellViewModel через RowViewModel.Presenter.ParentGrid.
        /// </summary>
        private static ColumnHeaderItem GetColumnHeaderForCell(CellViewModel cellVm)
        {
            if (cellVm?.Column == null) return null;
            var rowVm = cellVm.RowViewModel;
            if (rowVm?.Presenter?.ParentGrid == null) return null;
            return rowVm.Presenter.ParentGrid.GetColumnHeaderItem(cellVm.Column);
        }

        /// <summary>
        /// Проверяет, находится ли мышь над RowSeparatorGripper.
        /// Поднимается по визуальному дереву от originalSource до RowContainer
        /// и проверяет, есть ли среди предков RowSeparatorGripper.
        /// Используется для предотвращения выделения строки при перетаскивании Gripper.
        /// </summary>
        private static bool IsMouseOverGripper(DependencyObject originalSource)
        {
            if (originalSource == null) return false;

            DependencyObject current = originalSource;
            while (current != null && !(current is RowContainer))
            {
                if (current is RowSeparatorGripper)
                    return true;
                current = VisualTreeHelper.GetParent(current);
            }
            return false;
        }

        /// <summary>
        /// Устанавливает обработчик для события RowClicked.
        /// Предварительно отписывает предыдущий обработчик, если он был установлен.
        /// </summary>
        public void SetRowClickedHandler(Action<RowViewModel> handler)
        {
            if (_rowClickedHandler != null)
                RowClicked -= _rowClickedHandler;
            _rowClickedHandler = handler;
            if (handler != null)
                RowClicked += handler;
        }

        /// <summary>
        /// Устанавливает обработчик для события RowDoubleClicked.
        /// Предварительно отписывает предыдущий обработчик, если он был установлен.
        /// </summary>
        public void SetRowDoubleClickedHandler(Action<RowViewModel> handler)
        {
            if (_rowDoubleClickedHandler != null)
                RowDoubleClicked -= _rowDoubleClickedHandler;
            _rowDoubleClickedHandler = handler;
            if (handler != null)
                RowDoubleClicked += handler;
        }

        /// <summary>
        /// Устанавливает обработчик для события CellRightClicked.
        /// Предварительно отписывает предыдущий обработчик, если он был установлен.
        /// </summary>
        public void SetCellRightClickedHandler(Action<RowViewModel, ColumnHeaderItem> handler)
        {
            if (_cellRightClickedHandler != null)
                CellRightClicked -= _cellRightClickedHandler;
            _cellRightClickedHandler = handler;
            if (handler != null)
                CellRightClicked += handler;
        }

        /// <summary>
        /// Устанавливает обработчик для события CellClicked.
        /// Предварительно отписывает предыдущий обработчик, если он был установлен.
        /// </summary>
        public void SetCellClickedHandler(Action<CellViewModel> handler)
        {
            if (_cellClickedHandler != null)
                CellClicked -= _cellClickedHandler;
            _cellClickedHandler = handler;
            if (handler != null)
                CellClicked += handler;
        }

        /// <summary>
        /// Отписывает все обработчики событий кликов.
        /// Вызывается при переиспользовании контейнера (виртуализация).
        /// </summary>
        public void ClearClickHandlers()
        {
            if (_rowClickedHandler != null)
            {
                RowClicked -= _rowClickedHandler;
                _rowClickedHandler = null;
            }
            if (_rowDoubleClickedHandler != null)
            {
                RowDoubleClicked -= _rowDoubleClickedHandler;
                _rowDoubleClickedHandler = null;
            }
            if (_cellRightClickedHandler != null)
            {
                CellRightClicked -= _cellRightClickedHandler;
                _cellRightClickedHandler = null;
            }
            if (_cellClickedHandler != null)
            {
                CellClicked -= _cellClickedHandler;
                _cellClickedHandler = null;
            }
        }

        /// <summary>
        /// Полная очистка контейнера для переиспользования.
        /// Вызывает Dispose() для старой RowViewModel, очищает Binding-и,
        /// сбрасывает Content/DataContext, отписывает события.
        /// </summary>
        /// <param name="forceVm">
        /// Старая RowViewModel, которую нужно диспозить.
        /// Должна быть получена из DataContext ДО вызова Clear().
        /// Если не передана — пытается взять из DataContext (на случай,
        /// если WPF ещё не сбросил его).
        /// </param>
        public void Clear(RowViewModel forceVm = null)
        {
            // Получаем VM для Dispose() с приоритетом:
            // 1. forceVm — явно передан из PrepareContainerForItemOverride
            // 2. DataContext — если WPF ещё не сбросил его
            // 3. Content — если DataContext уже null, но Content ещё хранит VM
            //    (WPF сбрасывает DataContext и Content в разное время)
            // 4. _lastVm — сохранён из DataContextChanged
            var vm = forceVm
                  ?? DataContext as RowViewModel
                  ?? Content as RowViewModel
                  ?? _lastVm;

            // Шаг 1: Очищаем Binding-и ДО сброса DataContext.
            BindingOperations.ClearAllBindings(this);

            // Шаг 2: Освобождаем ресурсы RowViewModel.
            if (vm != null)
            {
                vm.Dispose();
                _lastVm = null;  // Сбрасываем после Dispose()
            }

            // Шаг 3: Сбрасываем Content/ContentTemplate.
            Content = null;
            ContentTemplate = null;

            // Шаг 4: Сбрасываем DataContext в null.
            DataContext = null;

            // Шаг 5: Отписываемся от PreviewMouseDown.
            PreviewMouseDown -= OnPreviewMouseDown;

            // Шаг 6: Отписываемся от DataContextChanged.
            DataContextChanged -= OnDataContextChanged;

            // Шаг 7: Отписываем обработчики кликов.
            ClearClickHandlers();

#if DEBUG
            MemoryDiagnostics.OnRowContainerCleared();
#endif
        }
    }
}

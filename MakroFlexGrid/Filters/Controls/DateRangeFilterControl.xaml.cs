using MakroFlexGrid.Headers;
using MakroFlexGrid.Utilities;
using System.Windows;
using System.Windows.Controls;

namespace MakroFlexGrid.Filters.Controls
{
    /// <summary>
    /// Элемент управления фильтром для колонок с датами.
    /// Позволяет задать диапазон дат (от... до).
    /// </summary>
    public partial class DateRangeFilterControl : UserControl
    {
        #region Private Variables

        private readonly ColumnHeaderItem _headerItem;
        private readonly FilterService _filterService;

        #endregion

        #region Constructor

        public DateRangeFilterControl(ColumnHeaderItem headerItem, FilterService filterService)
        {
            InitializeComponent();

            _headerItem = headerItem ?? throw new ArgumentNullException(nameof(headerItem));
            _filterService = filterService ?? throw new ArgumentNullException(nameof(filterService));

            // Устанавливаем заголовок
            HeaderText.Text = headerItem.Header?.ToString() ?? LocalizationManager.GetString("NoName", "(no name)");

            // Устанавливаем локализованные тексты
            FromLabel.Text = LocalizationManager.GetString("FilterFrom", "From:");
            ToLabel.Text = LocalizationManager.GetString("FilterTo", "To:");
            ApplyButton.Content = LocalizationManager.GetString("FilterApply", "Apply");
            ClearButton.Content = LocalizationManager.GetString("FilterClear", "Clear");

            // Восстанавливаем состояние фильтра
            RestoreFilterState();
        }

        #endregion

        #region Private Methods

        private void RestoreFilterState()
        {
            var existingFilter = _filterService.GetFilter(_headerItem);
            if (existingFilter is not DateColumnFilter dateFilter || !dateFilter.IsActive)
                return;

            if (dateFilter.FromDate.HasValue)
                FromDatePicker.SelectedDate = dateFilter.FromDate.Value;

            if (dateFilter.ToDate.HasValue)
                ToDatePicker.SelectedDate = dateFilter.ToDate.Value;
        }

        private void ApplyFilter()
        {
            var filter = new DateColumnFilter
            {
                SortMemberPath = _headerItem.SortMemberPath,
                DataType = _headerItem.SortDataType
            };

            if (FromDatePicker.SelectedDate.HasValue)
                filter.FromDate = FromDatePicker.SelectedDate.Value;

            if (ToDatePicker.SelectedDate.HasValue)
                filter.ToDate = ToDatePicker.SelectedDate.Value;

            if (filter.FromDate.HasValue || filter.ToDate.HasValue)
            {
                filter.Activate();
            }

            _filterService.SetFilter(_headerItem, filter);
        }

        private void CloseParentPopup()
        {
            var parent = Parent as System.Windows.Controls.Primitives.Popup;
            if (parent != null)
            {
                parent.IsOpen = false;
            }
        }

        #endregion

        #region Event Handlers

        private void OnFromDateChanged(object sender, SelectionChangedEventArgs e)
        {
            // Автоматически применяем фильтр при выборе даты (опционально)
        }

        private void OnToDateChanged(object sender, SelectionChangedEventArgs e)
        {
            // Автоматически применяем фильтр при выборе даты (опционально)
        }

        private void OnApplyClick(object sender, RoutedEventArgs e)
        {
            ApplyFilter();
            CloseParentPopup();
        }

        private void OnClearClick(object sender, RoutedEventArgs e)
        {
            _filterService.ClearFilter(_headerItem);
            FromDatePicker.SelectedDate = null;
            ToDatePicker.SelectedDate = null;
            CloseParentPopup();
        }

        #endregion
    }
}
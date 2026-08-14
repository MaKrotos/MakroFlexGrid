using MakroFlexGrid.Headers;
using MakroFlexGrid.Utilities;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace MakroFlexGrid.Filters.Controls
{
    /// <summary>
    /// Элемент управления фильтром для колонок с датой и временем.
    /// Позволяет задать диапазон дат и времени (от... до).
    /// </summary>
    public partial class DateTimeRangeFilterControl : UserControl
    {
        #region Private Variables

        private readonly ColumnHeaderItem _headerItem;
        private readonly FilterService _filterService;

        #endregion

        #region Constructor

        public DateTimeRangeFilterControl(ColumnHeaderItem headerItem, FilterService filterService)
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
            if (existingFilter is not DateTimeColumnFilter dateTimeFilter || !dateTimeFilter.IsActive)
                return;

            if (dateTimeFilter.FromDateTime.HasValue)
            {
                var from = dateTimeFilter.FromDateTime.Value;
                FromDatePicker.SelectedDate = from.Date;
                FromTimeTextBox.Text = from.ToString("HH:mm:ss");
            }

            if (dateTimeFilter.ToDateTime.HasValue)
            {
                var to = dateTimeFilter.ToDateTime.Value;
                ToDatePicker.SelectedDate = to.Date;
                ToTimeTextBox.Text = to.ToString("HH:mm:ss");
            }
        }

        private void ApplyFilter()
        {
            var filter = new DateTimeColumnFilter
            {
                SortMemberPath = _headerItem.SortMemberPath,
                DataType = _headerItem.SortDataType
            };

            if (FromDatePicker.SelectedDate.HasValue)
            {
                if (DateTime.TryParseExact(FromTimeTextBox.Text, "HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var time))
                {
                    filter.FromDateTime = FromDatePicker.SelectedDate.Value.Add(time.TimeOfDay);
                }
                else
                {
                    filter.FromDateTime = FromDatePicker.SelectedDate.Value;
                }
            }

            if (ToDatePicker.SelectedDate.HasValue)
            {
                if (DateTime.TryParseExact(ToTimeTextBox.Text, "HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var time))
                {
                    filter.ToDateTime = ToDatePicker.SelectedDate.Value.Add(time.TimeOfDay);
                }
                else
                {
                    filter.ToDateTime = ToDatePicker.SelectedDate.Value;
                }
            }

            if (filter.FromDateTime.HasValue || filter.ToDateTime.HasValue)
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
            FromTimeTextBox.Text = "00:00:00";
            ToDatePicker.SelectedDate = null;
            ToTimeTextBox.Text = "23:59:59";
            CloseParentPopup();
        }

        #endregion
    }
}
using MakroFlexGrid.Headers;
using MakroFlexGrid.Utilities;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MakroFlexGrid.Filters.Controls
{
    /// <summary>
    /// Элемент управления фильтром для числовых колонок.
    /// Позволяет задать диапазон значений (от... до).
    /// </summary>
    public partial class RangeFilterControl : UserControl
    {
        #region Private Variables

        private readonly ColumnHeaderItem _headerItem;
        private readonly FilterService _filterService;

        #endregion

        #region Constructor

        public RangeFilterControl(ColumnHeaderItem headerItem, FilterService filterService)
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
            if (existingFilter is not NumberColumnFilter numberFilter || !numberFilter.IsActive)
                return;

            if (numberFilter.FromValue.HasValue)
                FromTextBox.Text = numberFilter.FromValue.Value.ToString(CultureInfo.InvariantCulture);

            if (numberFilter.ToValue.HasValue)
                ToTextBox.Text = numberFilter.ToValue.Value.ToString(CultureInfo.InvariantCulture);
        }

        private void ApplyFilter()
        {
            var filter = new NumberColumnFilter
            {
                SortMemberPath = _headerItem.SortMemberPath,
                DataType = _headerItem.SortDataType
            };

            bool hasFrom = double.TryParse(FromTextBox.Text, NumberStyles.Any,
                CultureInfo.InvariantCulture, out double fromValue);
            bool hasTo = double.TryParse(ToTextBox.Text, NumberStyles.Any,
                CultureInfo.InvariantCulture, out double toValue);

            if (hasFrom)
                filter.FromValue = fromValue;

            if (hasTo)
                filter.ToValue = toValue;

            if (hasFrom || hasTo)
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

        private void OnPreviewNumberInput(object sender, TextCompositionEventArgs e)
        {
            // Разрешаем только цифры, точку и минус
            var regex = new Regex(@"^[0-9.\-]$");
            e.Handled = !regex.IsMatch(e.Text);
        }

        private void OnFromTextChanged(object sender, TextChangedEventArgs e)
        {
            // Автоматически применяем фильтр при вводе (опционально)
        }

        private void OnToTextChanged(object sender, TextChangedEventArgs e)
        {
            // Автоматически применяем фильтр при вводе (опционально)
        }

        private void OnApplyClick(object sender, RoutedEventArgs e)
        {
            ApplyFilter();
            CloseParentPopup();
        }

        private void OnClearClick(object sender, RoutedEventArgs e)
        {
            _filterService.ClearFilter(_headerItem);
            FromTextBox.Text = string.Empty;
            ToTextBox.Text = string.Empty;
            CloseParentPopup();
        }

        #endregion
    }
}
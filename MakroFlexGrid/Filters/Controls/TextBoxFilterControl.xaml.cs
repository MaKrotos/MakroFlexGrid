using MakroFlexGrid.Headers;
using MakroFlexGrid.Utilities;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace MakroFlexGrid.Filters.Controls
{
    /// <summary>
    /// Элемент управления фильтром для текстовых колонок.
    /// Позволяет искать по тексту и выбирать значения из списка.
    /// </summary>
    public partial class TextBoxFilterControl : UserControl, INotifyPropertyChanged
    {
        #region Private Variables

        private readonly ColumnHeaderItem _headerItem;
        private readonly FilterService _filterService;
        private readonly ObservableCollection<FilterValueItem> _filterValues = new();
        private string _searchText;

        #endregion

        #region Constructor

        public TextBoxFilterControl(ColumnHeaderItem headerItem, FilterService filterService)
        {
            InitializeComponent();

            _headerItem = headerItem ?? throw new ArgumentNullException(nameof(headerItem));
            _filterService = filterService ?? throw new ArgumentNullException(nameof(filterService));

            // Устанавливаем заголовок
            HeaderText.Text = headerItem.Header?.ToString() ?? LocalizationManager.GetString("NoName", "(no name)");

            // Заполняем выпадающий список операторов локализованными строками
            OperatorCombo.Items.Add(new ComboBoxItem { Content = LocalizationManager.GetString("FilterContains", "Contains") });
            OperatorCombo.Items.Add(new ComboBoxItem { Content = LocalizationManager.GetString("FilterEquals", "Equals") });
            OperatorCombo.Items.Add(new ComboBoxItem { Content = LocalizationManager.GetString("FilterNotEquals", "Not Equals") });
            OperatorCombo.Items.Add(new ComboBoxItem { Content = LocalizationManager.GetString("FilterStartsWith", "Starts With") });
            OperatorCombo.Items.Add(new ComboBoxItem { Content = LocalizationManager.GetString("FilterEndsWith", "Ends With") });

            // Устанавливаем текст кнопок
            ApplyButton.Content = LocalizationManager.GetString("FilterApply", "Apply");
            ClearButton.Content = LocalizationManager.GetString("FilterClear", "Clear");

            // Загружаем уникальные значения
            LoadUniqueValues();

            // Восстанавливаем состояние фильтра, если он уже был установлен
            RestoreFilterState();

            // Подписываемся на изменение текста поиска
            SearchTextBox.TextChanged += OnSearchTextChanged;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Коллекция значений для отображения в списке.
        /// </summary>
        public ObservableCollection<FilterValueItem> FilterValues => _filterValues;

        /// <summary>
        /// Текст поиска для фильтрации списка значений.
        /// </summary>
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged();
                    ApplySearchFilter();
                }
            }
        }

        #endregion

        #region Private Methods

        private void LoadUniqueValues()
        {
            _filterValues.Clear();

            var uniqueValues = _filterService.GetUniqueValues(_headerItem);
            foreach (var value in uniqueValues)
            {
                _filterValues.Add(new FilterValueItem
                {
                    DisplayValue = value?.ToString() ?? LocalizationManager.GetString("Empty", "(empty)"),
                    RawValue = value,
                    IsSelected = false
                });
            }
        }

        private void RestoreFilterState()
        {
            var existingFilter = _filterService.GetFilter(_headerItem);
            if (existingFilter is not TextColumnFilter textFilter || !textFilter.IsActive)
                return;

            // Восстанавливаем текстовый оператор
            if (textFilter.TextOperator != FilterOperator.In)
            {
                foreach (var item in OperatorCombo.Items)
                {
                    if (item is ComboBoxItem comboItem &&
                        comboItem.Content?.ToString() == GetOperatorDisplayName(textFilter.TextOperator))
                    {
                        OperatorCombo.SelectedItem = comboItem;
                        break;
                    }
                }

                SearchTextBox.Text = textFilter.TextValue;
            }

            // Восстанавливаем выбранные значения
            if (textFilter.SelectedValues.Count > 0)
            {
                foreach (var filterItem in _filterValues)
                {
                    if (textFilter.SelectedValues.Contains(filterItem.RawValue) ||
                        textFilter.SelectedValues.Contains(filterItem.DisplayValue))
                    {
                        filterItem.IsSelected = true;
                    }
                }
            }
        }

        private void ApplySearchFilter()
        {
            if (string.IsNullOrEmpty(SearchText))
            {
                // Показываем все значения
                foreach (var item in _filterValues)
                    item.IsVisible = true;
            }
            else
            {
                // Фильтруем по тексту
                foreach (var item in _filterValues)
                {
                    item.IsVisible = item.DisplayValue.IndexOf(SearchText,
                        StringComparison.CurrentCultureIgnoreCase) >= 0;
                }
            }
        }

        private void ApplyFilter()
        {
            var filter = new TextColumnFilter
            {
                SortMemberPath = _headerItem.SortMemberPath,
                DataType = _headerItem.SortDataType
            };

            // Определяем, какой режим фильтрации используется
            var selectedValues = _filterValues.Where(v => v.IsSelected).ToList();

            if (selectedValues.Count > 0)
            {
                // Режим фильтра по списку значений
                filter.TextOperator = FilterOperator.In;
                foreach (var item in selectedValues)
                {
                    if (item.RawValue != null)
                        filter.SelectedValues.Add(item.RawValue);
                }
                filter.Activate();
            }
            else if (!string.IsNullOrEmpty(SearchTextBox.Text))
            {
                // Режим текстового поиска
                filter.TextOperator = GetSelectedOperator();
                filter.TextValue = SearchTextBox.Text;
                filter.Activate();
            }

            _filterService.SetFilter(_headerItem, filter);
        }

        private FilterOperator GetSelectedOperator()
        {
            if (OperatorCombo.SelectedItem is ComboBoxItem selectedItem)
            {
                string content = selectedItem.Content?.ToString() ?? "";
                return content switch
                {
                    "Equals" or "Равно" => FilterOperator.Equals,
                    "Not Equals" or "Не равно" => FilterOperator.NotEquals,
                    "Starts With" or "Начинается с" => FilterOperator.StartsWith,
                    "Ends With" or "Заканчивается на" => FilterOperator.EndsWith,
                    _ => FilterOperator.Contains
                };
            }
            return FilterOperator.Contains;
        }

        private static string GetOperatorDisplayName(FilterOperator op)
        {
            return op switch
            {
                FilterOperator.Equals => LocalizationManager.GetString("FilterEquals", "Equals"),
                FilterOperator.NotEquals => LocalizationManager.GetString("FilterNotEquals", "Not Equals"),
                FilterOperator.Contains => LocalizationManager.GetString("FilterContains", "Contains"),
                FilterOperator.StartsWith => LocalizationManager.GetString("FilterStartsWith", "Starts With"),
                FilterOperator.EndsWith => LocalizationManager.GetString("FilterEndsWith", "Ends With"),
                _ => LocalizationManager.GetString("FilterContains", "Contains")
            };
        }

        #endregion

        #region Event Handlers

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            SearchText = SearchTextBox.Text;
        }

        private void OnValueChecked(object sender, RoutedEventArgs e)
        {
            // При выборе значения из списка, очищаем текстовый поиск
            if (sender is CheckBox checkBox && checkBox.DataContext is FilterValueItem)
            {
                SearchTextBox.Text = string.Empty;
            }
        }

        private void OnValueUnchecked(object sender, RoutedEventArgs e)
        {
            // Ничего не делаем специально
        }

        private void OnApplyClick(object sender, RoutedEventArgs e)
        {
            ApplyFilter();

            // Закрываем родительское Popup-окно
            CloseParentPopup();
        }

        private void OnClearClick(object sender, RoutedEventArgs e)
        {
            _filterService.ClearFilter(_headerItem);

            // Сбрасываем UI
            SearchTextBox.Text = string.Empty;
            OperatorCombo.SelectedIndex = 0;
            foreach (var item in _filterValues)
                item.IsSelected = false;

            CloseParentPopup();
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

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    /// <summary>
    /// Элемент списка значений фильтра.
    /// </summary>
    public class FilterValueItem : INotifyPropertyChanged
    {
        private string _displayValue;
        private object _rawValue;
        private bool _isSelected;
        private bool _isVisible = true;

        /// <summary>
        /// Отображаемое значение.
        /// </summary>
        public string DisplayValue
        {
            get => _displayValue;
            set { _displayValue = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Исходное значение (для сравнения).
        /// </summary>
        public object RawValue
        {
            get => _rawValue;
            set { _rawValue = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Выбран ли элемент.
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Видим ли элемент (для фильтрации списка).
        /// </summary>
        public bool IsVisible
        {
            get => _isVisible;
            set { _isVisible = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
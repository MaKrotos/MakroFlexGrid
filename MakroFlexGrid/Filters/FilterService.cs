using MakroFlexGrid.Core;
using MakroFlexGrid.Headers;
using System.Reflection;
using System.Windows.Data;

namespace MakroFlexGrid.Filters
{
    /// <summary>
    /// Центральный сервис управления фильтрацией колонок.
    /// Управляет коллекцией активных фильтров и применяет их к ICollectionView.
    /// </summary>
    public class FilterService
    {
        #region Private Variables

        private readonly Dictionary<string, ColumnFilterBase> _filters = new();
        private readonly WeakReference<CustomDataGrid> _grid;
        private Predicate<object> _currentFilter;

        #endregion

        #region Events

        /// <summary>
        /// Событие, уведомляющее об изменении фильтра.
        /// Подписчики (например, CustomDataGrid) должны обновить отображение.
        /// </summary>
        public event Action FilterChanged;

        #endregion

        #region Constructor

        public FilterService(CustomDataGrid grid)
        {
            _grid = new WeakReference<CustomDataGrid>(grid ?? throw new ArgumentNullException(nameof(grid)));
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Устанавливает фильтр для указанной колонки.
        /// </summary>
        public void SetFilter(ColumnHeaderItem headerItem, ColumnFilterBase filter)
        {
            if (headerItem == null) throw new ArgumentNullException(nameof(headerItem));

            string key = GetFilterKey(headerItem);

            if (filter != null && filter.IsActive)
            {
                _filters[key] = filter;
                headerItem.Filter = filter;
            }
            else
            {
                _filters.Remove(key);
                headerItem.Filter = null;
            }

            ApplyFilters();
            FilterChanged?.Invoke();
        }

        /// <summary>
        /// Сбрасывает фильтр для указанной колонки.
        /// </summary>
        public void ClearFilter(ColumnHeaderItem headerItem)
        {
            if (headerItem == null) throw new ArgumentNullException(nameof(headerItem));

            string key = GetFilterKey(headerItem);
            _filters.Remove(key);

            if (headerItem.Filter != null)
            {
                headerItem.Filter.Clear();
                headerItem.Filter = null;
            }

            ApplyFilters();
            FilterChanged?.Invoke();
        }

        /// <summary>
        /// Сбрасывает все активные фильтры.
        /// </summary>
        public void ClearAllFilters()
        {
            _filters.Clear();

            // Очищаем Filter во всех заголовках
            if (_grid.TryGetTarget(out var grid))
            {
                ClearFiltersInCollection(grid.FrozenColumnHeaders);
                ClearFiltersInCollection(grid.ScrollableColumnHeaders);
                ClearFiltersInCollection(grid.RightFrozenColumnHeaders);
            }

            ApplyFilters();
            FilterChanged?.Invoke();
        }

        /// <summary>
        /// Возвращает активный фильтр для колонки, или null если фильтр не установлен.
        /// </summary>
        public ColumnFilterBase GetFilter(ColumnHeaderItem headerItem)
        {
            if (headerItem == null) return null;
            string key = GetFilterKey(headerItem);
            return _filters.TryGetValue(key, out var filter) ? filter : null;
        }

        /// <summary>
        /// Собирает уникальные значения для указанной колонки из ItemsSource.
        /// </summary>
        public List<object> GetUniqueValues(ColumnHeaderItem headerItem)
        {
            var values = new HashSet<object>();

            if (headerItem == null || string.IsNullOrEmpty(headerItem.SortMemberPath))
                return values.ToList();

            if (!_grid.TryGetTarget(out var grid) || grid.ItemsSource == null)
                return values.ToList();

            var memberPath = headerItem.SortMemberPath;

            foreach (var item in grid.ItemsSource)
            {
                if (item == null) continue;

                var value = GetPropertyValue(item, memberPath);
                if (value != null)
                {
                    values.Add(value);
                }
            }

            // Сортируем значения
            var sortedList = values.ToList();
            sortedList.Sort((a, b) =>
            {
                if (a == null && b == null) return 0;
                if (a == null) return -1;
                if (b == null) return 1;
                return string.Compare(a.ToString(), b.ToString(), StringComparison.CurrentCultureIgnoreCase);
            });

            return sortedList;
        }

        /// <summary>
        /// Применяет все активные фильтры к ICollectionView.
        /// </summary>
        public void ApplyFilters()
        {
            if (!_grid.TryGetTarget(out var grid) || grid.ItemsSource == null)
                return;

            var view = CollectionViewSource.GetDefaultView(grid.ItemsSource);

            if (_filters.Count == 0)
            {
                // Сбрасываем фильтр
                view.Filter = null;
                _currentFilter = null;
                view.Refresh();
                return;
            }

            // Создаём предикат, который проверяет элемент по всем активным фильтрам
            _currentFilter = item => PassesAllFilters(item);
            view.Filter = _currentFilter;
            view.Refresh();
        }

        /// <summary>
        /// Проверяет, есть ли хотя бы один активный фильтр.
        /// </summary>
        public bool HasActiveFilters => _filters.Count > 0;

        /// <summary>
        /// Возвращает количество активных фильтров.
        /// </summary>
        public int ActiveFilterCount => _filters.Count;

        #endregion

        #region Private Methods

        private static string GetFilterKey(ColumnHeaderItem headerItem)
        {
            // Используем SortMemberPath как ключ, так как он уникален для каждой колонки
            return headerItem.SortMemberPath ?? headerItem.GetHashCode().ToString();
        }

        private bool PassesAllFilters(object item)
        {
            if (item == null) return false;

            foreach (var filter in _filters.Values)
            {
                if (filter == null || !filter.IsActive)
                    continue;

                var value = GetPropertyValue(item, filter.SortMemberPath);
                if (!filter.Passes(value))
                    return false;
            }

            return true;
        }

        private static object GetPropertyValue(object item, string propertyPath)
        {
            if (item == null || string.IsNullOrEmpty(propertyPath))
                return null;

            // Поддержка вложенных свойств (например, "Address.City")
            var properties = propertyPath.Split('.');
            object current = item;

            foreach (var prop in properties)
            {
                if (current == null) return null;

                var type = current.GetType();
                var property = type.GetProperty(prop, BindingFlags.Public | BindingFlags.Instance);
                if (property == null) return null;

                current = property.GetValue(current);
            }

            return current;
        }

        private static void ClearFiltersInCollection(ColumnHeaderCollection collection)
        {
            foreach (var item in collection)
            {
                if (item.Filter != null)
                {
                    item.Filter.Clear();
                    item.Filter = null;
                }

                if (item.HasChildren)
                {
                    ClearFiltersInCollection(item.Children);
                }
            }
        }

        #endregion
    }
}
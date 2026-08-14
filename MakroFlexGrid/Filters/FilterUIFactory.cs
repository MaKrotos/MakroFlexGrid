using MakroFlexGrid.Filters.Controls;
using MakroFlexGrid.Headers;
using MakroFlexGrid.Sorting;
using System.Windows;

namespace MakroFlexGrid.Filters
{
    /// <summary>
    /// Фабрика для создания UI элементов управления фильтрацией
    /// в зависимости от типа данных колонки (SortDataType).
    /// </summary>
    public static class FilterUIFactory
    {
        /// <summary>
        /// Создаёт элемент управления фильтром для указанной колонки.
        /// </summary>
        /// <param name="headerItem">Элемент заголовка колонки.</param>
        /// <param name="filterService">Сервис фильтрации.</param>
        /// <returns>UI элемент управления фильтром.</returns>
        public static UIElement CreateFilterControl(ColumnHeaderItem headerItem, FilterService filterService)
        {
            if (headerItem == null)
                throw new ArgumentNullException(nameof(headerItem));

            if (filterService == null)
                throw new ArgumentNullException(nameof(filterService));

            switch (headerItem.SortDataType)
            {
                case SortDataType.Text:
                    return new TextBoxFilterControl(headerItem, filterService);

                case SortDataType.Number:
                    return new RangeFilterControl(headerItem, filterService);

                case SortDataType.Date:
                    return new DateRangeFilterControl(headerItem, filterService);

                case SortDataType.DateTime:
                    return new DateTimeRangeFilterControl(headerItem, filterService);

                default:
                    // По умолчанию — текстовый фильтр
                    return new TextBoxFilterControl(headerItem, filterService);
            }
        }
    }
}
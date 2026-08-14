namespace MakroFlexGrid.Sorting
{
    /// <summary>
    /// Тип данных для сортировки колонки.
    /// </summary>
    public enum SortDataType
    {
        /// <summary>
        /// Сортировка как текст (по умолчанию).
        /// </summary>
        Text,

        /// <summary>
        /// Сортировка как число.
        /// </summary>
        Number,

        /// <summary>
        /// Сортировка как дата.
        /// </summary>
        Date,
        DateTime,

        /// <summary>
        /// Сортировка как логическое значение (true/false).
        /// </summary>
        Boolean
    }
}

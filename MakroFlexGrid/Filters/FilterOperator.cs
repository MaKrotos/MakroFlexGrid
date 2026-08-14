namespace MakroFlexGrid.Filters
{
    /// <summary>
    /// Операторы фильтрации для колонок.
    /// Определяет тип сравнения при фильтрации данных.
    /// </summary>
    public enum FilterOperator
    {
        /// <summary>
        /// Фильтр не применяется.
        /// </summary>
        None,

        /// <summary>
        /// Равно (==).
        /// </summary>
        Equals,

        /// <summary>
        /// Не равно (!=).
        /// </summary>
        NotEquals,

        /// <summary>
        /// Содержит подстроку (для текстовых полей).
        /// </summary>
        Contains,

        /// <summary>
        /// Начинается с подстроки.
        /// </summary>
        StartsWith,

        /// <summary>
        /// Заканчивается на подстроку.
        /// </summary>
        EndsWith,

        /// <summary>
        /// Больше (>).
        /// </summary>
        GreaterThan,

        /// <summary>
        /// Меньше (<).
        /// </summary>
        LessThan,

        /// <summary>
        /// Больше или равно (>=).
        /// </summary>
        GreaterThanOrEqual,

        /// <summary>
        /// Меньше или равно (<=).
        /// </summary>
        LessThanOrEqual,

        /// <summary>
        /// В диапазоне (от... до).
        /// </summary>
        Between,

        /// <summary>
        /// Входит в список значений.
        /// </summary>
        In
    }
}